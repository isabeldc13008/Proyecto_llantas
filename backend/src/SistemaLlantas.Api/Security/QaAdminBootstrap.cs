using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Api.Security;

public static class QaAdminBootstrap
{
    public static readonly IReadOnlyList<string> ExpectedPermissions = Array.AsReadOnly(new[] {
        "modulos.resumen.consultar", "modulos.administracion.consultar", "modulos.auditoria.consultar",
        "catalogos.administrar", "centros.ver_todos" });

    public static bool IsEnabled(string environment, IConfiguration config) =>
        environment == "QA" && config.GetValue<bool>("Authentication") &&
        config.GetValue<bool>("Authentication:QaAdminBootstrap:Enabled");

    public static void ValidateTarget(string connectionString, string expectedServer)
    {
        var c = new SqlConnectionStringBuilder(connectionString);
        if (c.InitialCatalog != "GDLLSQLPRBS" || string.IsNullOrWhiteSpace(expectedServer) ||
            !string.Equals(c.DataSource, expectedServer, StringComparison.OrdinalIgnoreCase) ||
            c.FailoverPartner.Length != 0 || c.AttachDBFilename.Length != 0 || c.UserInstance ||
            c.Encrypt == SqlConnectionEncryptOption.Optional || c.TrustServerCertificate)
            throw new InvalidOperationException("Destino QA o transporte del bootstrap no autorizado.");
    }

    public interface IStore
    {
        Task<bool> UserExists(CancellationToken ct);
        Task<(Guid Id, bool Active, string[] Permissions)?> ReadRole(CancellationToken ct);
        Task Insert(UsuarioSistema user, CancellationToken ct);
    }

    // Caller must own the database transaction and distributed lock throughout this operation.
    public static async Task<bool> CreateIfMissing(IStore store, string? password, CancellationToken ct = default)
    {
        if (await store.UserExists(ct)) return false; // Never reset, reactivate or repair an existing identity.
        var role = await store.ReadRole(ct);
        if (role is null || !role.Value.Active || ExpectedPermissions.Except(role.Value.Permissions, StringComparer.Ordinal).Any())
            throw new InvalidOperationException("ADMINISTRADOR ausente, inactivo o sin los permisos activos requeridos.");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 16 || password.Length > 256)
            throw new InvalidOperationException("Configure el secreto inicial QA (16 a 256 caracteres).");
        var user = new UsuarioSistema { Username = "admin.qa", Nombre = "Administrador QA", RolId = role.Value.Id,
            Activo = true, DebeCambiarClave = true, UsuarioCreacion = "qa-admin-bootstrap" };
        var hasher = new PasswordHasher<UsuarioSistema>();
        user.PasswordHash = hasher.HashPassword(user, password);
        if (hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Success)
            throw new InvalidOperationException("No fue posible verificar el hash inicial.");
        await store.Insert(user, ct);
        return true;
    }

    public static async Task RunAsync(string environment, IConfiguration config, CancellationToken ct = default)
    {
        if (!IsEnabled(environment, config)) return;
        try
        {
            var connectionString = config.GetConnectionString("SqlServer") ?? "";
            ValidateTarget(connectionString, config["Authentication:QaAdminBootstrap:ExpectedServer"] ?? "");
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(ct);
            await using (var identity = connection.CreateCommand())
            {
                identity.CommandText = "SELECT DB_NAME();";
                if ((string?)await identity.ExecuteScalarAsync(ct) != "GDLLSQLPRBS")
                    throw new InvalidOperationException("Base conectada no autorizada.");
            }
            await using var tx = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            await using (var gate = connection.CreateCommand())
            {
                gate.Transaction = tx;
                gate.CommandText = """
                    DECLARE @r int;
                    EXEC @r=sys.sp_getapplock @Resource=N'GLLD-QA-ADMIN-BOOTSTRAP',
                        @LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=15000;
                    IF @r<0 THROW 51000,'No se obtuvo bloqueo de bootstrap.',1;
                    """;
                await gate.ExecuteNonQueryAsync(ct);
            }
            await CreateIfMissing(new SqlStore(connection, tx), config["Authentication:QaAdminBootstrap:InitialPassword"], ct);
            await tx.CommitAsync(ct);
        }
        catch (Exception) when (!ct.IsCancellationRequested)
        {
            // No exception payload/inner exception: SQL/config errors may include sensitive values.
            throw new InvalidOperationException("Bootstrap QA no completado. Revisar destino, secreto, rol y permisos con el operador autorizado.");
        }
    }

    private sealed class SqlStore(SqlConnection connection, SqlTransaction tx) : IStore
    {
        private SqlCommand Command(string text) => new(text, connection, tx);
        public async Task<bool> UserExists(CancellationToken ct)
        {
            await using var c = Command("SELECT COUNT_BIG(*) FROM dbo.TBL_Usuario WITH(UPDLOCK,HOLDLOCK) WHERE LOWER(LTRIM(RTRIM(SUsername)))=N'admin.qa';");
            return (long)(await c.ExecuteScalarAsync(ct))! > 0;
        }
        public async Task<(Guid, bool, string[])?> ReadRole(CancellationToken ct)
        {
            await using var c = Command("""
                SELECT r.GId,r.BActivo,p.SCodigo
                FROM dbo.TBL_Rol r WITH(HOLDLOCK)
                LEFT JOIN dbo.TBL_RolPermiso rp WITH(HOLDLOCK) ON rp.GRolId=r.GId
                LEFT JOIN dbo.TBL_Permiso p WITH(HOLDLOCK) ON p.GId=rp.GPermisoId AND p.BActivo=1
                WHERE r.SCodigo=N'ADMINISTRADOR';
                """);
            await using var reader = await c.ExecuteReaderAsync(ct);
            Guid? id = null; bool active = false; var permissions = new List<string>();
            while (await reader.ReadAsync(ct))
            {
                if (id.HasValue && id.Value != reader.GetGuid(0)) throw new InvalidOperationException("Rol ambiguo.");
                id = reader.GetGuid(0); active = reader.GetBoolean(1);
                if (!reader.IsDBNull(2)) permissions.Add(reader.GetString(2));
            }
            return id.HasValue ? (id.Value, active, permissions.ToArray()) : null;
        }
        public async Task Insert(UsuarioSistema u, CancellationToken ct)
        {
            await using var c = Command("""
                INSERT dbo.TBL_Usuario(GId,SUsername,SNombre,SPasswordHash,GRolId,GCentroId,
                  DFechaCreacion,SUsuarioCreacion,BActivo,BDebeCambiarClave)
                VALUES(@id,N'admin.qa',N'Administrador QA',@hash,@role,NULL,
                  SYSDATETIMEOFFSET(),N'qa-admin-bootstrap',1,1);
                """);
            c.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = u.Id;
            c.Parameters.Add("@role", SqlDbType.UniqueIdentifier).Value = u.RolId;
            c.Parameters.Add("@hash", SqlDbType.NVarChar, 500).Value = u.PasswordHash;
            await c.ExecuteNonQueryAsync(ct);
        }
    }
}
