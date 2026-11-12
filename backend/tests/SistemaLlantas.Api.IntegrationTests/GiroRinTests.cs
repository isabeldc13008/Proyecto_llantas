using Microsoft.AspNetCore.Authorization;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Services;
namespace SistemaLlantas.Api.IntegrationTests;

// Offline tests: no host, database connection or migrations.
public sealed class GiroRinTests
{
    private static (GiroRinDto Dto, AsignacionLlantaPosicion Assignment, UsuarioSistema Technician) Scenario()
    {
        var tire = new Llanta("LL-1", null) { KilometrajeAcumulado = 700, UbicacionActual = "ABC / P1" };
        var vehicle = new Vehiculo { Placa = "ABC", Kilometraje = 1000, CentroId = Guid.NewGuid() };
        var axle = new EjeVehiculo { Vehiculo = vehicle, VehiculoId = vehicle.Id };
        var position = new PosicionVehiculo { Codigo = "P1", EjeVehiculo = axle, EjeVehiculoId = axle.Id, LlantaActualId = tire.Id };
        var assignment = new AsignacionLlantaPosicion { Llanta = tire, LlantaId = tire.Id, PosicionVehiculo = position, PosicionVehiculoId = position.Id, KilometrajeMontaje = 900, FechaInicio = DateTimeOffset.UtcNow.AddDays(-2) };
        var technician = new UsuarioSistema { Nombre = "Técnico", Username = "tecnico" };
        return (new(vehicle.Id, position.Id, tire.Id, 1100, technician.Id, DateTimeOffset.UtcNow.AddMinutes(-5), "Desgaste"), assignment, technician);
    }
    [Fact] public void GiroConservaAsignacionEstadoUbicacionYKilometrajeDeLlanta()
    {
        var (dto, a, tech) = Scenario(); var id = a.Id; var state = a.Llanta.EstadoLlantaId; var start = a.FechaInicio;
        var record = OperacionService.CrearRegistroGiro(dto, a, tech, "operador");
        Assert.Equal(dto.Fecha, record.FechaGiro); Assert.Equal(dto.Kilometraje, record.KilometrajeGiro); Assert.Equal(tech.Id, record.TecnicoGiroId);
        Assert.Equal("Giro en el mismo rin", record.Tipo); Assert.Equal(dto.Motivo, record.Motivo);
        Assert.Contains("1100 km", record.Observaciones); Assert.Contains("Técnico", record.Observaciones); Assert.Contains(dto.Fecha.ToString("O"), record.Observaciones);
        var detail = Assert.Single(record.Detalles); Assert.Equal(dto.PosicionId, detail.PosicionOrigenId); Assert.Equal(detail.PosicionOrigenId, detail.PosicionDestinoId); Assert.Equal(TipoDestinoLlanta.Posicion, detail.TipoDestino); Assert.Null(detail.CentroDestinoId);
        Assert.Equal(id, a.Id); Assert.True(a.EsActiva); Assert.Null(a.FechaFin); Assert.Equal(start, a.FechaInicio); Assert.Null(a.KilometrajeDesmontaje); Assert.Equal(900, a.KilometrajeMontaje);
        Assert.Equal(state, a.Llanta.EstadoLlantaId); Assert.Equal("ABC / P1", a.Llanta.UbicacionActual); Assert.Equal(700, a.Llanta.KilometrajeAcumulado); Assert.Equal(dto.LlantaId, a.PosicionVehiculo.LlantaActualId);
    }
    [Theory][InlineData("position")][InlineData("vehicle")][InlineData("tire")][InlineData("inactive")]
    public void RechazaAsignacionCambiada(string change)
    {
        var (dto,a,tech)=Scenario();if(change=="position")dto=dto with{PosicionId=Guid.NewGuid()};if(change=="vehicle")dto=dto with{VehiculoId=Guid.NewGuid()};if(change=="tire")a.PosicionVehiculo.LlantaActualId=Guid.NewGuid();if(change=="inactive")a.EsActiva=false;
        Assert.Throws<ConflictoException>(()=>OperacionService.CrearRegistroGiro(dto,a,tech,"operador"));Assert.Null(a.FechaFin);
    }
    [Theory][InlineData("mileage")][InlineData("date")][InlineData("future")][InlineData("technician")][InlineData("reason")]
    public void ValidaCamposAntesDeRegistrar(string invalid)
    {
        var(dto,a,tech)=Scenario();dto=invalid switch {"mileage"=>dto with{Kilometraje=999},"date"=>dto with{Fecha=DateTimeOffset.UtcNow.AddDays(-5)},"future"=>dto with{Fecha=DateTimeOffset.UtcNow.AddDays(1)},"technician"=>dto with{TecnicoId=Guid.Empty},_=>dto with{Motivo=" "}};
        Assert.Throws<ValidacionException>(()=>OperacionService.CrearRegistroGiro(dto,a,tech,"operador"));Assert.True(a.EsActiva);
    }
    [Theory][InlineData(nameof(OperacionesController.GiroRin))][InlineData(nameof(OperacionesController.TecnicosGiro))]
    public void ExigePermisoDeEjecucion(string method)=>Assert.Contains(typeof(OperacionesController).GetMethod(method)!.GetCustomAttributes(typeof(AuthorizeAttribute),true).Cast<AuthorizeAttribute>(),x=>x.Policy=="Operaciones.Ejecutar");

    [Theory][InlineData(true)][InlineData(false)]
    public async Task TecnicoAutenticadoNoPuedeRegistrarOtroTecnico(bool technician)
    {
        var (dto,_,_)=Scenario();var current=Guid.NewGuid();
        var service=System.Reflection.DispatchProxy.Create<IOperacionService,GiroCapture>();
        var capture=(GiroCapture)(object)service;
        var controller=new OperacionesController(service,null!,null!);
        controller.ControllerContext=new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext=new Microsoft.AspNetCore.Http.DefaultHttpContext { User=new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[]{new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier,current.ToString()),new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role,technician?"TECNICO":"ADMINISTRADOR"),new System.Security.Claims.Claim("username","actual")},"test")) }};
        await controller.GiroRin(dto,CancellationToken.None);
        Assert.Equal(technician?current:dto.TecnicoId,capture.Dto!.TecnicoId);
    }
    public class GiroCapture : System.Reflection.DispatchProxy
    {
        public GiroRinDto? Dto;
        protected override object? Invoke(System.Reflection.MethodInfo? method,object?[]? args)
        {
            Dto=(GiroRinDto)args![0]!;
            return Task.FromResult(new MovimientoDto(Guid.NewGuid(),"GIR","Giro en el mismo rin","M",DateTimeOffset.UtcNow,[]));
        }
    }
}
