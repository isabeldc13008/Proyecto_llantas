using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.Controllers;

[ApiController,Route("api/auth")]
public sealed class AuthController(LlantasDbContext db,IConfiguration config):ControllerBase
{
    [AllowAnonymous,HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request,CancellationToken ct)
    {
        var username=request.Username.Trim().ToLowerInvariant();
        var user=await db.UsuariosSistema.Include(x=>x.Centros).Include(x=>x.Rol).ThenInclude(x=>x.Permisos).ThenInclude(x=>x.Permiso).SingleOrDefaultAsync(x=>x.Username==username&&x.Activo&&x.Rol.Activo,ct);
        if(user is null||new PasswordHasher<UsuarioSistema>().VerifyHashedPassword(user,user.PasswordHash,request.Password)==PasswordVerificationResult.Failed)return Unauthorized(new{message="Usuario o contraseña incorrectos."});
        var claims=new List<Claim>{new(JwtRegisteredClaimNames.Sub,user.Id.ToString()),new(ClaimTypes.NameIdentifier,user.Id.ToString()),new("username",user.Username),new(ClaimTypes.Name,user.Nombre),new(ClaimTypes.Role,user.Rol.Codigo),new("rol_nombre",user.Rol.Nombre)};
        claims.AddRange(user.Centros.Where(x=>x.Activo).Select(x=>new Claim("centro_id",x.CentroId.ToString())));
        claims.AddRange(user.Rol.Permisos.Where(x=>x.Permiso.Activo).Select(x=>new Claim("permiso",x.Permiso.Codigo)));
        var key=config["Jwt:Key"]??throw new InvalidOperationException("Jwt:Key no configurada.");var expires=DateTime.UtcNow.AddHours(8);
        var token=new JwtSecurityToken(config["Jwt:Issuer"],config["Jwt:Audience"],claims,expires:expires,signingCredentials:new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),SecurityAlgorithms.HmacSha256));
        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token),expires,user.Nombre,user.Username,user.Rol.Codigo,user.Rol.Nombre,Initials(user.Nombre),claims.Where(x=>x.Type=="permiso").Select(x=>x.Value).ToArray(),user.Centros.Where(x=>x.Activo).Select(x=>x.CentroId).ToArray(),claims.Any(x=>x.Type=="permiso"&&x.Value=="centros.ver_todos"));
    }
    private static string Initials(string name)=>string.Concat(name.Split(' ',StringSplitOptions.RemoveEmptyEntries).Take(2).Select(x=>char.ToUpperInvariant(x[0])));
    public sealed record LoginRequest(string Username,string Password);
    public sealed record LoginResponse(string AccessToken,DateTime ExpiresAt,string Name,string Username,string Role,string RoleName,string Initials,string[] Permissions,Guid[] CenterIds,bool CanViewAllCenters);
}
