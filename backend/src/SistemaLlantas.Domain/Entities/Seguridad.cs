using SistemaLlantas.Domain.Common;

namespace SistemaLlantas.Domain.Entities;

public sealed class RolSistema : EntidadAuditable
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public ICollection<RolPermiso> Permisos { get; set; } = [];
    public ICollection<UsuarioSistema> Usuarios { get; set; } = [];
}
public sealed class PermisoSistema : EntidadAuditable
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public ICollection<RolPermiso> Roles { get; set; } = [];
}
public sealed class RolPermiso
{
    public Guid RolId { get; set; } public RolSistema Rol { get; set; } = null!;
    public Guid PermisoId { get; set; } public PermisoSistema Permiso { get; set; } = null!;
}
public sealed class UsuarioSistema : EntidadAuditable
{
    public string Username { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Guid RolId { get; set; } public RolSistema Rol { get; set; } = null!;
    public Guid? CentroId { get; set; } public Centro? Centro { get; set; }
    public ICollection<UsuarioCentro> Centros { get; set; } = [];
}

public sealed class UsuarioCentro : EntidadAuditable
{
    public Guid UsuarioId { get; set; }
    public UsuarioSistema Usuario { get; set; } = null!;
    public Guid CentroId { get; set; }
    public Centro Centro { get; set; } = null!;
}
