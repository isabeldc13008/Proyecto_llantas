using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class InspectionNationalScopeTests
{
    [Theory]
    [InlineData(true, "inspector", EstadoInspeccion.Borrador, true)]
    [InlineData(false, "inspector", EstadoInspeccion.Borrador, false)]
    [InlineData(true, "otro", EstadoInspeccion.Borrador, false)]
    [InlineData(true, "inspector", EstadoInspeccion.Finalizada, false)]
    public void CorreccionNacionalExigePermisoPropiedadYBorrador(bool crear, string propietario, EstadoInspeccion estado, bool permitido)
    {
        // Build and translate the real query without opening a database connection.
        using var db = new LlantasDbContext(new DbContextOptionsBuilder<LlantasDbContext>()
            .UseSqlServer("Server=unused;Database=unused;Integrated Security=true").Options);
        var centroLocal=Guid.NewGuid();var centroVehiculo=Guid.NewGuid();
        var inspeccion=new Inspeccion {Activo=true,TecnicoId=propietario,Estado=estado,Vehiculo=new Vehiculo {Activo=true,CentroId=centroVehiculo}};
        var id=inspeccion.Id;
        var claims=new List<Claim>{new("username","inspector"),new("centro_id",centroLocal.ToString())};
        if(crear)claims.Add(new("permiso","inspecciones.crear"));
        var user=new ClaimsPrincipal(new ClaimsIdentity(claims,"test"));
        var controller=new InspeccionesController(null!,db,null!) { ControllerContext=new ControllerContext {HttpContext=new DefaultHttpContext {User=user}} };
        var query=(IQueryable<Inspeccion>)typeof(InspeccionesController).GetMethod("InspeccionAsignable",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(controller,[id])!;
        Assert.Contains("WHERE",query.ToQueryString());
        var call=(MethodCallExpression)query.Expression;
        var predicate=(Expression<Func<Inspeccion,bool>>)((UnaryExpression)call.Arguments[1]).Operand;
        Assert.Equal(permitido,predicate.Compile()(inspeccion));
        Assert.False(user.AlcanceCentros().Autoriza(centroVehiculo)); // Other modules retain their original scope.
        Assert.False(user.HasClaim("permiso","llantas.administrar"));
    }
}
