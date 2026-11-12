using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Domain.Entities;
namespace SistemaLlantas.Application.Tests;
public class RevisionPosicionMontajeTests
{
 private static SolicitudOperacion Row()=>new(){Tipo="Montaje",Estado=EstadoSolicitudOperacion.PENDIENTE_APROBACION,Solicitante="tecnico",LlantaId=Guid.NewGuid(),PosicionDestinoId=Guid.NewGuid(),RowVersion=[1,2],DestinoDesplazada="Inventario"};
 private static DecisionPosicionDto Decision(string action="APROBAR",string? reason=null,DestinoRetirada? d=null)=>new(action,"AQI=",reason,d);
 [Fact] public void ApprovalDoesNotExecuteOrChangeOtherPositions(){var a=Row();var b=Row();RevisionPosicionMontaje.Decidir(a,Decision(),"supervisor",false);Assert.Equal(EstadoSolicitudOperacion.APROBADO,a.Estado);Assert.Equal(EstadoSolicitudOperacion.PENDIENTE_APROBACION,b.Estado);Assert.Null(a.MovimientoEjecutadoId);Assert.Null(a.Ejecutor);Assert.Null(a.FechaEjecucion);Assert.Contains("APROBAR",a.HistorialRevisionJson!);}
 [Fact] public void ReturnAndResubmitPreserveAudit(){var s=Row();RevisionPosicionMontaje.Decidir(s,Decision("DEVOLVER","Cambiar llanta"),"supervisor",false);Assert.Equal(EstadoSolicitudOperacion.DEVUELTO,s.Estado);RevisionPosicionMontaje.Corregir(s,new("AQI=",Guid.NewGuid(),null,"Corregido",null),"tecnico");Assert.Equal(EstadoSolicitudOperacion.PENDIENTE_APROBACION,s.Estado);Assert.Null(s.Aprobador);Assert.Contains("DEVOLVER",s.HistorialRevisionJson!);Assert.Contains("REENVIAR",s.HistorialRevisionJson!);}
 [Fact] public void DestinationChangeRecordsBeforeAfter(){var s=Row();s.LlantaDesplazadaId=Guid.NewGuid();RevisionPosicionMontaje.Decidir(s,Decision("MODIFICAR_DESTINO","Inspección",new("Reparacion")),"supervisor",false);Assert.Equal("Reparacion",s.DestinoDesplazada);Assert.Contains("Inventario",s.DestinoSolicitadoJson!);Assert.Contains("Reparacion",s.HistorialRevisionJson!);}
 [Theory][InlineData("DEVOLVER")][InlineData("MODIFICAR_DESTINO")] public void JustificationRequired(string action)=>Assert.Throws<ValidacionException>(()=>RevisionPosicionMontaje.Decidir(Row(),Decision(action),"supervisor",false));
 [Fact] public void SelfApprovalIsForbidden()=>Assert.Throws<UnauthorizedAccessException>(()=>RevisionPosicionMontaje.Decidir(Row(),Decision(),"tecnico",false));
 [Fact] public void StaleVersionCannotApprove()=>Assert.Throws<ConflictoException>(()=>RevisionPosicionMontaje.Decidir(Row(),new("APROBAR","old",null),"supervisor",false));
 [Theory][InlineData(EstadoSolicitudOperacion.PENDIENTE_APROBACION)][InlineData(EstadoSolicitudOperacion.DEVUELTO)][InlineData(EstadoSolicitudOperacion.EJECUTADO)][InlineData(EstadoSolicitudOperacion.RECHAZADO)] public void ExecutionRejectsNonApproved(EstadoSolicitudOperacion state){var s=Row();s.Estado=state;Assert.Throws<ConflictoException>(()=>RevisionPosicionMontaje.ValidarEjecucion([s]));}
 [Fact] public void ExecutionRejectsCompletedMovement(){var s=Row();s.Estado=EstadoSolicitudOperacion.APROBADO;s.MovimientoEjecutadoId=Guid.NewGuid();Assert.Throws<ConflictoException>(()=>RevisionPosicionMontaje.ValidarEjecucion([s]));}
 [Fact] public void CannotCorrectAnotherRequest(){var s=Row();s.Estado=EstadoSolicitudOperacion.DEVUELTO;Assert.Throws<UnauthorizedAccessException>(()=>RevisionPosicionMontaje.Corregir(s,new("AQI=",Guid.NewGuid(),null,"Motivo",null),"otro"));}
 [Fact] public void CannotCorrectApprovedRow(){var s=Row();s.Estado=EstadoSolicitudOperacion.APROBADO;Assert.Throws<ConflictoException>(()=>RevisionPosicionMontaje.Corregir(s,new("AQI=",Guid.NewGuid(),null,"Motivo",null),"tecnico"));}
 [Theory][InlineData("DisposicionFinal")][InlineData("Otro")][InlineData("Posicion")][InlineData("Traslado")] public void InvalidDestinationsFail(string value)=>Assert.Throws<ValidacionException>(()=>RevisionPosicionMontaje.ValidarDestino(new(value),true));
 [Theory][InlineData("Inventario")][InlineData("Reparacion")][InlineData("Reencauche")] public void SupportedDestinations(string value)=>RevisionPosicionMontaje.ValidarDestino(new(value),true);
 [Fact] public void OccupiedPositionRequiresExplicitDestination()=>Assert.Throws<ValidacionException>(()=>RevisionPosicionMontaje.ValidarDestino(null,true));
 [Fact] public void VacantPositionCannotHaveTransferDestination()=>Assert.Throws<ValidacionException>(()=>RevisionPosicionMontaje.ValidarDestino(new("Traslado",CentroId:Guid.NewGuid()),false));
}
