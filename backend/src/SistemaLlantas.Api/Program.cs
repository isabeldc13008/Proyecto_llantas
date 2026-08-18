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
if (builder.Environment.IsDevelopment())
    builder.Services.AddAuthentication(DevelopmentAuthenticationHandler.Scheme).AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(DevelopmentAuthenticationHandler.Scheme, _ => { });
else
{
    var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Configure Jwt:Key mediante secretos o variables de entorno.");
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
    o.AddPolicy("Llantas.Consultar", p => p.RequireClaim("permiso", "llantas.consultar", "llantas.administrar"));
    o.AddPolicy("Llantas.Administrar", p => p.RequireClaim("permiso", "llantas.administrar"));
    o.AddPolicy("Catalogos.Administrar", p => p.RequireClaim("permiso", "catalogos.administrar"));
    o.AddPolicy("Inspecciones.Consultar", p => p.RequireClaim("permiso", "inspecciones.consultar", "inspecciones.crear", "inspecciones.autorizar_inconsistencia_llanta"));
    o.AddPolicy("Inspecciones.Crear", p => p.RequireClaim("permiso", "inspecciones.crear"));
    o.AddPolicy("Inspecciones.ReportarInconsistencia", p => p.RequireClaim("permiso", "inspecciones.reportar_inconsistencia", "inspecciones.crear"));
    o.AddPolicy("Inspecciones.AutorizarInconsistencia", p => p.RequireClaim("permiso", "inspecciones.autorizar_inconsistencia_llanta"));
    o.AddPolicy("Operaciones.Ejecutar", p => p.RequireClaim("permiso", "operaciones.ejecutar", "llantas.administrar"));
    o.AddPolicy("Actividades.ConsultarPropias", p => p.RequireAuthenticatedUser());
});
builder.Services.AddCors(o => o.AddPolicy("Angular", p => p.WithOrigins(builder.Configuration["FrontendUrl"] ?? "http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseMiddleware<ApiExceptionMiddleware>();
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseCors("Angular"); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
app.Run();

public partial class Program { }
