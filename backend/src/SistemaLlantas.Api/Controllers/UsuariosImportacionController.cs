using SistemaLlantas.Api.Security;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net.Mail;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.Controllers;

[ApiController, Route("api/usuarios/importar"), Authorize(Roles = "ADMINISTRADOR")]
[RequestSizeLimit(4_000_000)]
public sealed class UsuariosImportacionController(LlantasDbContext db,IConfiguration config) : ControllerBase
{
    [HttpPost("validar")]
    public async Task<ActionResult<Validacion>> Validar(ValidarCsv dto, CancellationToken ct) =>
        await Analizar(dto.Csv, ct);

    [HttpPost]
    public async Task<IActionResult> Importar(ImportarCsv dto, CancellationToken ct)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        try
        {
            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                db.ChangeTracker.Clear();
                await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
                var resultado = await Analizar(dto.Csv, ct);
                if (string.IsNullOrWhiteSpace(dto.Password))
                    resultado.Errores.Add(new(0, "Contraseña", "Contraseña requerida para el lote.", ""));
                if (!resultado.PuedeImportar) return BadRequest(resultado);
                foreach (var fila in resultado.Filas)
                {
                    var user = UsuariosController.CrearEntidad(new(fila.Correo, fila.Nombre,
                        dto.Password, fila.RolId!.Value, fila.CentroIds), User.Identity?.Name ?? "sistema");
                    user.Activo = fila.Estado!.Value;
                    user.DebeCambiarClave = LocalPasswordChangeMiddleware.EsLocal(config);
                    db.UsuariosSistema.Add(user);
                }
                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return Ok(new { importados = resultado.Total });
            });
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "No se importó ningún usuario. Hubo un conflicto al guardar; vuelve a validar el archivo." });
        }
    }

    private async Task<Validacion> Analizar(string csv, CancellationToken ct)
    {
        var result = LeerCsv(csv);
        if (result.Filas.Count == 0) return result;
        var roles = await db.RolesSistema.AsNoTracking().Select(x => new { x.Id, x.Codigo, x.Activo }).ToListAsync(ct);
        var centros = await db.Centros.AsNoTracking().Select(x => new { x.Id, x.Codigo, x.Activo }).ToListAsync(ct);
        var correos = result.Filas.Select(x => x.Correo.ToLowerInvariant()).Distinct().ToArray();
        var existentes = (await db.UsuariosSistema.AsNoTracking().Where(x => correos.Contains(x.Username.ToLower()))
            .Select(x => x.Username).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var duplicados = result.Filas.Where(x => x.Correo.Length > 0).GroupBy(x => x.Correo, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var fila in result.Filas)
        {
            void Error(string campo, string error, string valor) => result.Errores.Add(new(fila.Fila, campo, error, fila.ValorOriginal(campo, valor)));
            if (duplicados.Contains(fila.Correo)) Error("Correo", "Correo duplicado dentro del archivo.", fila.Correo);
            if (existentes.Contains(fila.Correo)) Error("Correo", "El usuario ya existe; no se actualizará.", fila.Correo);
            var matches = roles.Where(x => string.Equals(x.Codigo.Trim(), fila.Rol, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (matches.Length != 1) Error("Rol", "Código de rol inexistente o ambiguo.", fila.Rol);
            else if (!matches[0].Activo) Error("Rol", "El rol está inactivo.", fila.Rol);
            else fila.RolId = matches[0].Id;
            var ids = new List<Guid>();
            foreach (var codigo in fila.Centro.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var found = centros.Where(x => string.Equals(x.Codigo.Trim(), codigo, StringComparison.OrdinalIgnoreCase)).ToArray();
                if (found.Length != 1) Error("Centro", $"Código de centro inexistente o ambiguo: {codigo}.", codigo);
                else if (!found[0].Activo) Error("Centro", $"El centro {codigo} está inactivo.", codigo);
                else ids.Add(found[0].Id);
            }
            fila.CentroIds = ids.ToArray();
        }
        return result;
    }

    // TextFieldParser supports quoted commas, escaped quotes and multiline fields without additional packages.
    internal static Validacion LeerCsv(string csv)
    {
        var result = new Validacion();
        if (Encoding.UTF8.GetByteCount(csv) > 1_000_000) { result.Errores.Add(new(1, "Archivo", "Máximo 1 MB de texto y 1000 usuarios.", "")); return result; }
        var lines = csv.Split('\n');
        using var reader = new TextFieldParser(new StringReader(csv.TrimStart('\uFEFF')))
        { TextFieldType = FieldType.Delimited, HasFieldsEnclosedInQuotes = true, TrimWhiteSpace = false };
        reader.SetDelimiters(",");
        try
        {
            var header = reader.ReadFields();
            if (header is null || !header.Select(x => x.Trim()).SequenceEqual(new[] { "Correo", "Nombre", "Rol", "Centro", "Activo" }, StringComparer.OrdinalIgnoreCase))
            { result.Errores.Add(new(1, "Cabecera", "Se requiere: Correo,Nombre,Rol,Centro,Activo", header is null ? "" : string.Join(",", header))); return result; }
        }
        catch (MalformedLineException)
        { result.Errores.Add(new(1, "Cabecera", "CSV mal formado.", reader.ErrorLine ?? "")); return result; }
        while (!reader.EndOfData)
        {
            var number = (int)reader.LineNumber;
            while (number > 0 && number <= lines.Length && string.IsNullOrWhiteSpace(lines[number - 1])) number++;
            if (result.Filas.Count >= 1000) { result.Errores.Add(new(number, "Archivo", "Máximo 1000 usuarios; divide el archivo.", "")); break; }
            string[]? fields;
            try { fields = reader.ReadFields(); }
            catch (MalformedLineException)
            {
                number = (int)reader.ErrorLineNumber;
                result.Filas.Add(new() { Fila = number });
                result.Errores.Add(new(number, "Fila", "CSV mal formado: revisa comillas y separadores.", reader.ErrorLine ?? ""));
                continue;
            }
            if (fields is null) break;
            var row = new FilaCsv { Fila = number, Correo = fields.ElementAtOrDefault(0)?.Trim().ToLowerInvariant() ?? "",
                Nombre = fields.ElementAtOrDefault(1)?.Trim() ?? "", Rol = fields.ElementAtOrDefault(2)?.Trim() ?? "",
                Centro = fields.ElementAtOrDefault(3)?.Trim() ?? "", Activo = fields.ElementAtOrDefault(4)?.Trim() ?? "", ValoresRecibidos = fields };
            result.Filas.Add(row);
            void Error(string campo, string error, string valor) => result.Errores.Add(new(number, campo, error, row.ValorOriginal(campo, valor)));
            if (fields.Length != 5) Error("Fila", "La fila debe tener exactamente cinco columnas.", string.Join(",", fields));
            if (row.Correo.Length == 0) Error("Correo", "Correo obligatorio.", row.Correo);
            else if (row.Correo.Length > 80 || !new EmailAddressAttribute().IsValid(row.Correo) ||
                !MailAddress.TryCreate(row.Correo, out var address) || address.Address != row.Correo)
                Error("Correo", "Correo inválido o mayor de 80 caracteres.", row.Correo);
            if (row.Nombre.Length == 0 || row.Nombre.Length > 150) Error("Nombre", "Nombre obligatorio, máximo 150 caracteres.", row.Nombre);
            if (row.Rol.Length == 0) Error("Rol", "Rol obligatorio.", row.Rol);
            if (bool.TryParse(row.Activo, out var active)) row.Estado = active;
            else Error("Activo", "Usa true o false.", row.Activo);
        }
        if (result.Filas.Count == 0) result.Errores.Add(new(2, "Archivo", "El archivo no contiene usuarios.", ""));
        return result;
    }

    public sealed record ValidarCsv(string Csv);
    public sealed record ImportarCsv(string Csv, string? Password);
    public sealed record ErrorCsv(int Fila, string Campo, string Error, string ValorRecibido);
    public sealed class FilaCsv
    {
        public int Fila { get; set; }
        public string Correo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Rol { get; set; } = "";
        public string Centro { get; set; } = "";
        public string Activo { get; set; } = "";
        [JsonIgnore] public Guid? RolId { get; set; }
        [JsonIgnore] public Guid[] CentroIds { get; set; } = [];
        [JsonIgnore] public bool? Estado { get; set; }
        [JsonIgnore] public string[] ValoresRecibidos { get; set; } = [];
        internal string ValorOriginal(string campo, string fallback) =>
            ValoresRecibidos.ElementAtOrDefault(campo switch { "Correo" => 0, "Nombre" => 1, "Rol" => 2, "Centro" => 3, "Activo" => 4, _ => -1 }) ?? fallback;
    }
    public sealed class Validacion
    {
        public List<FilaCsv> Filas { get; } = [];
        public List<ErrorCsv> Errores { get; } = [];
        public int Total => Filas.Count;
        public int ConError => Filas.Count(f => Errores.Any(e => e.Fila == f.Fila));
        public int Validas => Total - ConError;
        public bool PuedeImportar => Total > 0 && Errores.Count == 0;
    }
}
