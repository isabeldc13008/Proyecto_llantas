SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_WARNINGS ON;
SET ANSI_PADDING ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @now datetimeoffset=SYSDATETIMEOFFSET(), @user nvarchar(50)=N'seed-local';

IF NOT EXISTS(SELECT 1 FROM TBL_Marca WHERE Codigo=N'MIC') INSERT TBL_Marca(Id,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),@now,@user,1,N'MIC',N'Michelin');
IF NOT EXISTS(SELECT 1 FROM TBL_Marca WHERE Codigo=N'GDY') INSERT TBL_Marca(Id,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),@now,@user,1,N'GDY',N'Goodyear');
IF NOT EXISTS(SELECT 1 FROM TBL_Marca WHERE Codigo=N'BRG') INSERT TBL_Marca(Id,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),@now,@user,1,N'BRG',N'Bridgestone');

DECLARE @mic uniqueidentifier=(SELECT Id FROM TBL_Marca WHERE Codigo=N'MIC'), @gdy uniqueidentifier=(SELECT Id FROM TBL_Marca WHERE Codigo=N'GDY'), @brg uniqueidentifier=(SELECT Id FROM TBL_Marca WHERE Codigo=N'BRG');
IF NOT EXISTS(SELECT 1 FROM TBL_Referencia WHERE Codigo=N'XMULTID') INSERT TBL_Referencia(Id,MarcaId,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),@mic,@now,@user,1,N'XMULTID',N'X Multi D');
IF NOT EXISTS(SELECT 1 FROM TBL_Referencia WHERE Codigo=N'KMAXS') INSERT TBL_Referencia(Id,MarcaId,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),@gdy,@now,@user,1,N'KMAXS',N'KMAX S');
IF NOT EXISTS(SELECT 1 FROM TBL_Referencia WHERE Codigo=N'R268') INSERT TBL_Referencia(Id,MarcaId,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),@brg,@now,@user,1,N'R268',N'R268 Ecopia');

IF NOT EXISTS(SELECT 1 FROM TBL_Dimension WHERE Codigo=N'29580R225') INSERT TBL_Dimension(Id,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),@now,@user,1,N'29580R225',N'295/80 R22.5');
IF NOT EXISTS(SELECT 1 FROM TBL_Dimension WHERE Codigo=N'31580R225') INSERT TBL_Dimension(Id,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),@now,@user,1,N'31580R225',N'315/80 R22.5');
IF NOT EXISTS(SELECT 1 FROM TBL_Dimension WHERE Codigo=N'12R225') INSERT TBL_Dimension(Id,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),@now,@user,1,N'12R225',N'12 R22.5');

IF NOT EXISTS(SELECT 1 FROM TBL_TipoLlanta WHERE Codigo=N'RAD') INSERT TBL_TipoLlanta(Id,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),@now,@user,1,N'RAD',N'Radial');
IF NOT EXISTS(SELECT 1 FROM TBL_TipoLlanta WHERE Codigo=N'DIR') INSERT TBL_TipoLlanta(Id,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),@now,@user,1,N'DIR',N'Direccional');
IF NOT EXISTS(SELECT 1 FROM TBL_TipoLlanta WHERE Codigo=N'TRA') INSERT TBL_TipoLlanta(Id,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),@now,@user,1,N'TRA',N'Tracción');

IF NOT EXISTS(SELECT 1 FROM TBL_EstadoLlanta WHERE Codigo=N'DIS') INSERT TBL_EstadoLlanta(Id,EsDisposicionFinal,PermiteMontaje,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),0,1,@now,@user,1,N'DIS',N'Disponible');
IF NOT EXISTS(SELECT 1 FROM TBL_EstadoLlanta WHERE Codigo=N'MON') INSERT TBL_EstadoLlanta(Id,EsDisposicionFinal,PermiteMontaje,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),0,0,@now,@user,1,N'MON',N'Montada');
IF NOT EXISTS(SELECT 1 FROM TBL_EstadoLlanta WHERE Codigo=N'REP') INSERT TBL_EstadoLlanta(Id,EsDisposicionFinal,PermiteMontaje,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),0,0,@now,@user,1,N'REP',N'En reparación');
IF NOT EXISTS(SELECT 1 FROM TBL_EstadoLlanta WHERE Codigo=N'REE') INSERT TBL_EstadoLlanta(Id,EsDisposicionFinal,PermiteMontaje,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),0,0,@now,@user,1,N'REE',N'En reencauche');
IF NOT EXISTS(SELECT 1 FROM TBL_EstadoLlanta WHERE Codigo=N'DFI') INSERT TBL_EstadoLlanta(Id,EsDisposicionFinal,PermiteMontaje,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),1,0,@now,@user,1,N'DFI',N'Disposición final');

IF NOT EXISTS(SELECT 1 FROM TBL_CondicionLlanta WHERE Codigo=N'SIN') INSERT TBL_CondicionLlanta(Id,RequiereCausa,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),0,@now,@user,1,N'SIN',N'Sin novedad');
IF NOT EXISTS(SELECT 1 FROM TBL_CondicionLlanta WHERE Codigo=N'DES') INSERT TBL_CondicionLlanta(Id,RequiereCausa,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),1,@now,@user,1,N'DES',N'Desgaste irregular');
IF NOT EXISTS(SELECT 1 FROM TBL_CondicionLlanta WHERE Codigo=N'COR') INSERT TBL_CondicionLlanta(Id,RequiereCausa,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),1,@now,@user,1,N'COR',N'Corte');
IF NOT EXISTS(SELECT 1 FROM TBL_CondicionLlanta WHERE Codigo=N'PER') INSERT TBL_CondicionLlanta(Id,RequiereCausa,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),1,@now,@user,1,N'PER',N'Perforación');
IF NOT EXISTS(SELECT 1 FROM TBL_CondicionLlanta WHERE Codigo=N'GOL') INSERT TBL_CondicionLlanta(Id,RequiereCausa,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),1,@now,@user,1,N'GOL',N'Golpe');
IF NOT EXISTS(SELECT 1 FROM TBL_CondicionLlanta WHERE Codigo=N'DEF') INSERT TBL_CondicionLlanta(Id,RequiereCausa,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),1,@now,@user,1,N'DEF',N'Deformación');
IF NOT EXISTS(SELECT 1 FROM TBL_CondicionLlanta WHERE Codigo=N'SEP') INSERT TBL_CondicionLlanta(Id,RequiereCausa,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),1,@now,@user,1,N'SEP',N'Separación');
IF NOT EXISTS(SELECT 1 FROM TBL_CondicionLlanta WHERE Codigo=N'OTR') INSERT TBL_CondicionLlanta(Id,RequiereCausa,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),1,@now,@user,1,N'OTR',N'Otro');

IF NOT EXISTS(SELECT 1 FROM TBL_CausaLlanta WHERE Codigo=N'ALI') INSERT TBL_CausaLlanta(Id,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),@now,@user,1,N'ALI',N'Problema de alineación');
IF NOT EXISTS(SELECT 1 FROM TBL_CausaLlanta WHERE Codigo=N'PRE') INSERT TBL_CausaLlanta(Id,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),@now,@user,1,N'PRE',N'Presión incorrecta');
IF NOT EXISTS(SELECT 1 FROM TBL_CausaLlanta WHERE Codigo=N'SOB') INSERT TBL_CausaLlanta(Id,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),@now,@user,1,N'SOB',N'Sobrecarga');
IF NOT EXISTS(SELECT 1 FROM TBL_CausaLlanta WHERE Codigo=N'SUS') INSERT TBL_CausaLlanta(Id,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),@now,@user,1,N'SUS',N'Daño en suspensión');
IF NOT EXISTS(SELECT 1 FROM TBL_CausaLlanta WHERE Codigo=N'EXT') INSERT TBL_CausaLlanta(Id,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),@now,@user,1,N'EXT',N'Elemento externo');
IF NOT EXISTS(SELECT 1 FROM TBL_CausaLlanta WHERE Codigo=N'PDE') INSERT TBL_CausaLlanta(Id,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),@now,@user,1,N'PDE',N'Por determinar');

IF NOT EXISTS(SELECT 1 FROM TBL_RecomendacionInspeccion WHERE Codigo=N'CON') INSERT TBL_RecomendacionInspeccion(Id,EsCandidataReencauche,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),0,@now,@user,1,N'CON',N'Continuar operación');
IF NOT EXISTS(SELECT 1 FROM TBL_RecomendacionInspeccion WHERE Codigo=N'SEG') INSERT TBL_RecomendacionInspeccion(Id,EsCandidataReencauche,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),0,@now,@user,1,N'SEG',N'Realizar seguimiento');
IF NOT EXISTS(SELECT 1 FROM TBL_RecomendacionInspeccion WHERE Codigo=N'ROT') INSERT TBL_RecomendacionInspeccion(Id,EsCandidataReencauche,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),0,@now,@user,1,N'ROT',N'Rotar');
IF NOT EXISTS(SELECT 1 FROM TBL_RecomendacionInspeccion WHERE Codigo=N'REP') INSERT TBL_RecomendacionInspeccion(Id,EsCandidataReencauche,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),0,@now,@user,1,N'REP',N'Reparar');
IF NOT EXISTS(SELECT 1 FROM TBL_RecomendacionInspeccion WHERE Codigo=N'DES') INSERT TBL_RecomendacionInspeccion(Id,EsCandidataReencauche,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),0,@now,@user,1,N'DES',N'Desmontar');
IF NOT EXISTS(SELECT 1 FROM TBL_RecomendacionInspeccion WHERE Codigo=N'REE') INSERT TBL_RecomendacionInspeccion(Id,EsCandidataReencauche,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),1,@now,@user,1,N'REE',N'Enviar a reencauche');
IF NOT EXISTS(SELECT 1 FROM TBL_RecomendacionInspeccion WHERE Codigo=N'SUP') INSERT TBL_RecomendacionInspeccion(Id,EsCandidataReencauche,FechaCreacion,UsuarioCreacion,Activo,Codigo,Nombre) VALUES(NEWID(),0,@now,@user,1,N'SUP',N'Revisar por supervisor');

DECLARE @dimension uniqueidentifier=(SELECT Id FROM TBL_Dimension WHERE Codigo=N'29580R225');
IF NOT EXISTS(SELECT 1 FROM TBL_ParametroReencauche WHERE DimensionId=@dimension AND VigenteHasta IS NULL) INSERT TBL_ParametroReencauche(Id,DimensionId,MaximoReencauches,ProfundidadMinima,VigenteDesde,VigenteHasta,FechaCreacion,UsuarioCreacion,Activo) VALUES(NEWID(),@dimension,2,3.00,'2026-01-01',NULL,@now,@user,1);

DECLARE @mounted uniqueidentifier=(SELECT Id FROM TBL_EstadoLlanta WHERE Codigo=N'MON'), @radial uniqueidentifier=(SELECT Id FROM TBL_TipoLlanta WHERE Codigo=N'RAD'), @ref uniqueidentifier=(SELECT Id FROM TBL_Referencia WHERE Codigo=N'XMULTID');
DECLARE @vehicles TABLE(InternalNo nvarchar(50),Plate nvarchar(20),VehicleType nvarchar(100),CenterCode nvarchar(30),Axles int,Tires int);
INSERT @vehicles VALUES(N'1542',N'ABC123',N'Tractocamión',N'8092',3,10),(N'2450',N'JKL908',N'Camión rígido',N'8279',2,6),(N'3190',N'MNO317',N'Camión rígido',N'8092',2,6);
DECLARE @internal nvarchar(50),@plate nvarchar(20),@vehicleType nvarchar(100),@centerCode nvarchar(30),@axles int,@tires int;
DECLARE vc CURSOR LOCAL FAST_FORWARD FOR SELECT InternalNo,Plate,VehicleType,CenterCode,Axles,Tires FROM @vehicles;
OPEN vc; FETCH NEXT FROM vc INTO @internal,@plate,@vehicleType,@centerCode,@axles,@tires;
WHILE @@FETCH_STATUS=0 BEGIN
 DECLARE @center uniqueidentifier=(SELECT Id FROM TBL_Centro WHERE Codigo=@centerCode),@vehicle uniqueidentifier=(SELECT Id FROM TBL_Vehiculo WHERE NumeroInterno=@internal);
 IF @vehicle IS NULL BEGIN SET @vehicle=NEWID(); INSERT TBL_Vehiculo(Id,NumeroInterno,Placa,Tipo,CentroId,FechaCreacion,UsuarioCreacion,Activo) VALUES(@vehicle,@internal,@plate,@vehicleType,@center,@now,@user,1); END;
 DECLARE @a int=1;
 WHILE @a<=@axles BEGIN
  DECLARE @axle uniqueidentifier=(SELECT Id FROM TBL_EjeVehiculo WHERE VehiculoId=@vehicle AND Numero=@a);
  IF @axle IS NULL BEGIN SET @axle=NEWID(); INSERT TBL_EjeVehiculo(Id,VehiculoId,Numero,Nombre,FechaCreacion,UsuarioCreacion,Activo) VALUES(@axle,@vehicle,@a,CONCAT(N'EJE ',@a),@now,@user,1); END;
  DECLARE @first int=CASE WHEN @a=1 THEN 1 ELSE 3+((@a-2)*4) END,@last int=CASE WHEN @a=1 THEN 2 ELSE 6+((@a-2)*4) END,@p int;
  SET @p=@first;
  WHILE @p<=@last AND @p<=@tires BEGIN
   DECLARE @position uniqueidentifier=(SELECT Id FROM TBL_PosicionVehiculo WHERE EjeVehiculoId=@axle AND Codigo=CONCAT(N'P',@p));
   IF @position IS NULL BEGIN SET @position=NEWID(); INSERT TBL_PosicionVehiculo(Id,EjeVehiculoId,Codigo,Lado,Orden,LlantaActualId,FechaCreacion,UsuarioCreacion,Activo) VALUES(@position,@axle,CONCAT(N'P',@p),CASE WHEN @p%2=1 THEN N'Izquierdo' ELSE N'Derecho' END,@p,NULL,@now,@user,1); END;
   DECLARE @tireCode nvarchar(50), @tire uniqueidentifier;
   SET @tireCode=CONCAT(N'LL-',RIGHT(N'00000'+CONVERT(nvarchar(10),CONVERT(int,@internal)*10+@p),5));
   SET @tire=(SELECT Id FROM TBL_Llanta WHERE Codigo=@tireCode);
   IF @tire IS NULL BEGIN SET @tire=NEWID(); INSERT TBL_Llanta(Id,Codigo,Serial,MarcaId,ReferenciaId,DimensionId,TipoLlantaId,EstadoLlantaId,CentroId,UbicacionActual,FechaCompra,Costo,KilometrajeAcumulado,NumeroReencauches,ProfundidadInicial,FechaIngreso,Observaciones,FechaCreacion,UsuarioCreacion,Activo) VALUES(@tire,@tireCode,CONCAT(N'SER-',@internal,N'-',@p),@mic,@ref,@dimension,@radial,@mounted,@center,CONCAT(N'Interno ',@internal,N' · P',@p),NULL,NULL,0,0,16.00,CONVERT(date,@now),N'Dato local de prueba',@now,@user,1); END;
   UPDATE TBL_PosicionVehiculo SET LlantaActualId=@tire WHERE Id=@position AND (LlantaActualId IS NULL OR LlantaActualId=@tire);
   IF NOT EXISTS(SELECT 1 FROM TBL_AsignacionLlantaPosicion WHERE PosicionVehiculoId=@position AND EsActiva=1) INSERT TBL_AsignacionLlantaPosicion(Id,LlantaId,PosicionVehiculoId,FechaInicio,FechaFin,EsActiva,MovimientoOrigenId,FechaCreacion,UsuarioCreacion,Activo) VALUES(NEWID(),@tire,@position,@now,NULL,1,NEWID(),@now,@user,1);
   SET @p+=1;
  END;
  SET @a+=1;
 END;
 IF NOT EXISTS(SELECT 1 FROM TBL_ActividadProgramada WHERE VehiculoId=@vehicle AND TipoActividad=N'Inspección' AND Estado=0) INSERT TBL_ActividadProgramada(Id,TipoActividad,FechaProgramada,CentroId,VehiculoId,TecnicoId,Prioridad,Estado,FechaCreacion,UsuarioCreacion,Activo) VALUES(NEWID(),N'Inspección',DATEADD(day,1,@now),@center,@vehicle,N'tecnico.local',N'Media',0,@now,@user,1);
 FETCH NEXT FROM vc INTO @internal,@plate,@vehicleType,@centerCode,@axles,@tires;
END;
CLOSE vc; DEALLOCATE vc;

COMMIT TRANSACTION;
