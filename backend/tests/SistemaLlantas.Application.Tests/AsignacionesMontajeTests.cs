using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Operaciones;
namespace SistemaLlantas.Application.Tests;
public sealed class AsignacionesMontajeTests
{
 [Fact]public void SinAsignacion_NoPermiteProgramar()=>Assert.Throws<ValidacionException>(()=>AsignacionesMontaje.Validar([]));
 [Fact]public void LlantaDuplicada_Rechazada(){var tire=Guid.NewGuid();Assert.Throws<ValidacionException>(()=>AsignacionesMontaje.Validar([new(Guid.NewGuid(),tire,null),new(Guid.NewGuid(),tire,null)]));}
 [Fact]public void PosicionDuplicada_Rechazada(){var p=Guid.NewGuid();Assert.Throws<ValidacionException>(()=>AsignacionesMontaje.Validar([new(p,Guid.NewGuid(),null),new(p,Guid.NewGuid(),null)]));}
 [Fact]public void LlantaActualNoEsReemplazo(){var tire=Guid.NewGuid();Assert.Throws<ValidacionException>(()=>AsignacionesMontaje.Validar([new(Guid.NewGuid(),tire,tire)]));}
 [Fact]public void IndividualNoPermiteVarias()=>Assert.Throws<ValidacionException>(()=>AsignacionesMontaje.Validar([new(Guid.NewGuid(),Guid.NewGuid(),null),new(Guid.NewGuid(),Guid.NewGuid(),null)],true));
 [Fact]public void PermiteVariasPosicionesDistintas()=>AsignacionesMontaje.Validar([new(Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid()),new(Guid.NewGuid(),Guid.NewGuid(),null)]);
 [Fact]public void IdVacioRechazado()=>Assert.Throws<ValidacionException>(()=>AsignacionesMontaje.Validar([new(Guid.Empty,Guid.NewGuid(),null)]));
}
