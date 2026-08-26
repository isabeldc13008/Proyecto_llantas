using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SistemaLlantas.Api;
using SistemaLlantas.Api.Middleware;
using SistemaLlantas.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddInfrastructure(builder.Configuration);
{
    var jwtKey = builder.Configuration["Jwt:Key"] ?? (builder.Environment.IsDevelopment()?"development-only-key-change-me-2026-llantas":throw new InvalidOperationException("Configure Jwt:Key mediante secretos o variables de entorno."));
    builder.Configuration["Jwt:Key"]=jwtKey;
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o => o.TokenValidationParameters = new()
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SistemaLlantas",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "SistemaLlantas.Web",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), ClockSkew = TimeSpan.FromMinutes(1)
    });
}
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("Dashboard.Consultar",p=>p.RequireClaim("permiso","modulos.resumen.consultar"));
    o.AddPolicy("Llantas.Consultar", p => p.RequireClaim("permiso", "llantas.consultar", "llantas.administrar"));
    o.AddPolicy("Llantas.Administrar", p => p.RequireClaim("permiso", "llantas.administrar"));
    o.AddPolicy("Catalogos.Administrar", p => p.RequireClaim("permiso", "catalogos.administrar"));
    o.AddPolicy("Inspecciones.Consultar", p => p.RequireClaim("permiso", "inspecciones.consultar", "inspecciones.crear", "inspecciones.autorizar_inconsistencia_llanta"));
    o.AddPolicy("Inspecciones.Crear", p => p.RequireClaim("permiso", "inspecciones.crear"));
    o.AddPolicy("Inspecciones.ReportarInconsistencia", p => p.RequireClaim("permiso", "inspecciones.reportar_inconsistencia", "inspecciones.crear"));
    o.AddPolicy("Inspecciones.AutorizarInconsistencia", p => p.RequireClaim("permiso", "inspecciones.autorizar_inconsistencia_llanta"));
    o.AddPolicy("Operaciones.Ejecutar", p => p.RequireClaim("permiso", "operaciones.ejecutar", "llantas.administrar"));
    o.AddPolicy("Actividades.ConsultarPropias", p => p.RequireAuthenticatedUser());
    o.AddPolicy("Vehiculos.Consultar", p => p.RequireClaim("permiso", "vehiculos.consultar", "vehiculos.administrar"));
    o.AddPolicy("Vehiculos.Administrar", p => p.RequireClaim("permiso", "vehiculos.administrar"));
    o.AddPolicy("Alertas.Consultar",p=>p.RequireClaim("permiso","alertas.consultar","alertas.gestionar","alertas.descartar"));
    o.AddPolicy("Alertas.Gestionar",p=>p.RequireClaim("permiso","alertas.gestionar","alertas.descartar"));
    o.AddPolicy("Evidencias.Eliminar",p=>p.RequireClaim("permiso","inspecciones.gestionar_evidencias"));
    o.AddPolicy("Programacion.Consultar",p=>p.RequireClaim("permiso","programacion.consultar","programacion.administrar"));
    o.AddPolicy("Programacion.Administrar",p=>p.RequireClaim("permiso","programacion.administrar"));
    o.AddPolicy("Operaciones.Solicitar",p=>p.RequireClaim("permiso","operaciones.solicitar","operaciones.ejecutar","operaciones.aprobar"));
    o.AddPolicy("Operaciones.Aprobar",p=>p.RequireClaim("permiso","operaciones.aprobar"));
    o.AddPolicy("Operaciones.Montar",p=>p.RequireClaim("permiso","operaciones.montar"));
    o.AddPolicy("ServiciosLlanta.Consultar",p=>p.RequireClaim("permiso","servicios_llanta.consultar","servicios_llanta.gestionar"));
    o.AddPolicy("ServiciosLlanta.Gestionar",p=>p.RequireClaim("permiso","servicios_llanta.gestionar"));
    o.AddPolicy("ServiciosLlanta.Opcionar",p=>p.RequireClaim("permiso","servicios_llanta.opcionar","servicios_llanta.gestionar"));
    o.AddPolicy("CargaMasiva.Importar",p=>p.RequireClaim("permiso","carga_masiva.importar"));
    o.AddPolicy("Reportes.Exportar",p=>p.RequireClaim("permiso","reportes.exportar"));
});
builder.Services.AddCors(o => o.AddPolicy("Angular", p => p.WithOrigins(builder.Configuration["FrontendUrl"] ?? "http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
if(app.Environment.IsDevelopment())await DevelopmentSecuritySeeder.SeedAsync(app.Services);
app.UseMiddleware<ApiExceptionMiddleware>();
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseCors("Angular"); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
app.Run();

public partial class Program { }
