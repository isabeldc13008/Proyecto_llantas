using System.Text.Json;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Application.Common;
namespace SistemaLlantas.Application.Operaciones;

public sealed record DestinoRetirada(string Tipo,Guid? PosicionId=null,Guid? CentroId=null);
public sealed record DecisionPosicionDto(string Accion,string RowVersion,string? Motivo,DestinoRetirada? Destino=null);
public sealed record CorregirPosicionDto(string RowVersion,Guid LlantaId,DestinoRetirada? Destino,string Motivo,string? Observaciones,Guid? LlantaActualId=null);
public sealed record EjecutarPosicionesDto(decimal Kilometraje,IReadOnlyList<VersionPosicion> Posiciones);
public sealed record VersionPosicion(Guid Id,string RowVersion);
public sealed record RevisionMontaje(string Accion,string Usuario,DateTimeOffset Fecha,string? Motivo,string Anterior,string Nuevo);

public static class RevisionPosicionMontaje
{
 public static bool EsMontaje(SolicitudOperacion s)=>s.Tipo is "Montaje" or "Reemplazar llanta" or "Cambio de juego";
 public static DestinoRetirada Destino(SolicitudOperacion s)=>new(s.DestinoDesplazada??"Inventario",s.PosicionDestinoDesplazadaId,s.CentroDestinoId);
 public static void Version(SolicitudOperacion s,string version){if(string.IsNullOrWhiteSpace(version)||Convert.ToBase64String(s.RowVersion)!=version)throw new ConflictoException("La posición cambió; actualiza el trabajo antes de continuar.");}
 public static void ValidarDestino(DestinoRetirada? d,bool ocupada)
 {
  if(!ocupada){if(d is not null&&(d.Tipo!="Inventario"||d.PosicionId.HasValue||d.CentroId.HasValue))throw new ValidacionException("Una posición libre no tiene llanta retirada.");return;}
  if(d is null||d.Tipo is not ("Inventario" or "Reparacion" or "Reencauche" or "Posicion" or "Traslado"))throw new ValidacionException("Selecciona un destino permitido para la llanta retirada.");
  if((d.Tipo=="Posicion")!=d.PosicionId.HasValue||(d.Tipo=="Traslado")!=d.CentroId.HasValue||d.PosicionId==Guid.Empty||d.CentroId==Guid.Empty)throw new ValidacionException("El destino requiere únicamente su posición o centro correspondiente.");
 }
 public static void Registrar(SolicitudOperacion s,string accion,string usuario,string? motivo,string anterior)
 {
  var log=string.IsNullOrEmpty(s.HistorialRevisionJson)?new List<RevisionMontaje>():JsonSerializer.Deserialize<List<RevisionMontaje>>(s.HistorialRevisionJson)!;
  log.Add(new(accion,usuario,DateTimeOffset.UtcNow,motivo,anterior,Foto(s)));s.HistorialRevisionJson=JsonSerializer.Serialize(log);s.UsuarioModificacion=usuario;
 }
 public static string Foto(SolicitudOperacion s)=>JsonSerializer.Serialize(new{s.LlantaId,s.PosicionDestinoId,s.LlantaDesplazadaId,Destino=Destino(s),s.Motivo,s.Observaciones,Estado=s.Estado.ToString()});
 public static void AplicarDestino(SolicitudOperacion s,DestinoRetirada d){ValidarDestino(d,s.LlantaDesplazadaId.HasValue);s.DestinoDesplazada=d.Tipo;s.PosicionDestinoDesplazadaId=d.PosicionId;s.CentroDestinoId=d.CentroId;}
 public static void Decidir(SolicitudOperacion s,DecisionPosicionDto d,string usuario,bool aprobarPropia)
 {
  if(!EsMontaje(s)||!s.Activo||s.Estado!=EstadoSolicitudOperacion.PENDIENTE_APROBACION)throw new ConflictoException("La posición no está pendiente de revisión.");
  Version(s,d.RowVersion);
  if(s.Solicitante==usuario&&!aprobarPropia)throw new UnauthorizedAccessException("No puede resolver su propia solicitud.");
  if(d.Accion is not ("APROBAR" or "DEVOLVER" or "MODIFICAR_DESTINO"))throw new ValidacionException("Decisión inválida.");
  if((d.Motivo?.Length??0)>500||((d.Accion!="APROBAR")&&string.IsNullOrWhiteSpace(d.Motivo)))throw new ValidacionException("La devolución o cambio de destino exige justificación (máximo 500 caracteres).");
  var before=Foto(s);s.DestinoSolicitadoJson??=JsonSerializer.Serialize(Destino(s));
  if(d.Accion=="MODIFICAR_DESTINO"){if(d.Destino is null)throw new ValidacionException("Selecciona el destino aprobado.");AplicarDestino(s,d.Destino);}
  else if(d.Destino is not null)throw new ValidacionException("Para cambiar un destino utiliza Modificar destino.");
  s.Estado=d.Accion=="DEVOLVER"?EstadoSolicitudOperacion.DEVUELTO:EstadoSolicitudOperacion.APROBADO;
  s.Aprobador=usuario;s.FechaDecision=DateTimeOffset.UtcNow;s.MotivoRechazo=d.Motivo?.Trim();Registrar(s,d.Accion,usuario,d.Motivo,before);
 }
 public static void Corregir(SolicitudOperacion s,CorregirPosicionDto d,string usuario)
 {
  Version(s,d.RowVersion);
  if(!EsMontaje(s)||s.Estado!=EstadoSolicitudOperacion.DEVUELTO||!s.Activo)throw new ConflictoException("Solo se corrigen posiciones devueltas.");
  if(s.Solicitante!=usuario)throw new UnauthorizedAccessException("Solo el solicitante puede corregir esta posición.");
  if(d.LlantaId==Guid.Empty||d.LlantaId==d.LlantaActualId||string.IsNullOrWhiteSpace(d.Motivo)||d.Motivo.Length>500||(d.Observaciones?.Length??0)>1000)throw new ValidacionException("Revisa llanta, motivo y observaciones.");
  ValidarDestino(d.Destino,d.LlantaActualId.HasValue);var before=Foto(s);
  s.LlantaId=d.LlantaId;s.LlantaDesplazadaId=d.LlantaActualId;AplicarDestino(s,d.Destino??new("Inventario"));s.DestinoSolicitadoJson=JsonSerializer.Serialize(Destino(s));s.Motivo=d.Motivo.Trim();s.Observaciones=d.Observaciones;s.Estado=EstadoSolicitudOperacion.PENDIENTE_APROBACION;s.Aprobador=null;s.FechaDecision=null;s.MotivoRechazo=null;Registrar(s,"REENVIAR",usuario,d.Motivo,before);
 }
 public static void ValidarEjecucion(IEnumerable<SolicitudOperacion> rows)
 {
  if(!rows.Any()||rows.Any(s=>!s.Activo||!EsMontaje(s)||s.Estado!=EstadoSolicitudOperacion.APROBADO||s.MovimientoEjecutadoId.HasValue||s.FechaEjecucion.HasValue))throw new ConflictoException("Solo se ejecutan posiciones aprobadas y no ejecutadas.");
 }
}
