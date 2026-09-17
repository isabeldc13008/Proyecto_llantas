using System.Globalization;
using System.ComponentModel.DataAnnotations;
using SistemaLlantas.Application.Llantas;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Vehiculos;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.Controllers;

[ApiController,Route("api/carga-masiva"),Authorize(Policy="CargaMasiva.Importar")]
public sealed class CargaMasivaController(LlantasDbContext db,IVehiculoService vehiculos):ControllerBase
{
 private static readonly string[] TireColumns=["CodigoLlanta","Serial","Marca","Referencia","Dimension","TipoLlanta","Estado","Centro","Ubicacion","ProfundidadInicial"];
 private static readonly string[] VehicleColumns=["Interno","Placa","Centro","TipoVehiculo","ConfiguracionEjes","Kilometraje","Estado"];
 [HttpGet("plantillas")]public object Plantillas()=>new[]{new{tipo="llantas",columnas=TireColumns},new{tipo="vehiculos",columnas=VehicleColumns}};
 [HttpGet("plantillas/{tipo}")]public IActionResult Plantilla(string tipo,[FromQuery]string formato="xlsx"){var cols=Columns(tipo);if(formato.Equals("csv",StringComparison.OrdinalIgnoreCase))return File(Encoding.UTF8.GetBytes(string.Join(',',cols)+Environment.NewLine),"text/csv",$"plantilla-{tipo}.csv");using var wb=new XLWorkbook();var ws=wb.AddWorksheet("Plantilla");for(var i=0;i<cols.Length;i++)ws.Cell(1,i+1).Value=cols[i];ws.Row(1).Style.Font.Bold=true;ws.Columns().AdjustToContents();using var ms=new MemoryStream();wb.SaveAs(ms);return File(ms.ToArray(),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",$"plantilla-{tipo}.xlsx");}
 [HttpPost("previsualizar"),RequestSizeLimit(26_000_000)]public async Task<ActionResult<PreviewDto>> Preview([FromForm]string tipo,[FromForm]IFormFile archivo,CancellationToken ct){if(archivo.Length is 0 or >25_000_000)throw new ValidacionException("El archivo está vacío o supera 25 MB.");var ext=Path.GetExtension(archivo.FileName).ToLowerInvariant();if(ext is not ".csv" and not ".xlsx")throw new ValidacionException("Sólo se permiten CSV y XLSX.");var rows=await Read(archivo,ext,ct);var required=Columns(tipo);if(rows.Count==0||required.Any(c=>!rows[0].ContainsKey(c)))throw new ValidacionException("La estructura no coincide con la plantilla vigente.");var errors=await Validate(tipo,rows,User.AlcanceCentros(),ct);var invalid=errors.Select(x=>x.Fila).ToHashSet();var valid=rows.Select((x,i)=>(Row:x,Number:i+2)).Where(x=>!invalid.Contains(x.Number)).Select(x=>x.Row).ToList();var load=new CargaMasiva{Tipo=tipo.ToLowerInvariant(),NombreArchivo=Path.GetFileName(archivo.FileName),TotalFilas=rows.Count,FilasValidas=valid.Count,FilasConError=invalid.Count,FilasJson=JsonSerializer.Serialize(valid),ErroresJson=JsonSerializer.Serialize(errors),Usuario=User.Username(),UsuarioCreacion=User.Username()};db.CargasMasivas.Add(load);await db.SaveChangesAsync(ct);return new PreviewDto(load.Id,load.TotalFilas,load.FilasValidas,load.FilasConError,errors.Take(100).ToList());}
 [HttpPost("{id:guid}/confirmar")]public async Task<ResultadoDto> Confirm(Guid id,CancellationToken ct){var result=new ResultadoDto(id,0,0);var strategy=db.Database.CreateExecutionStrategy();await strategy.ExecuteAsync(async()=>{await using var tx=await db.Database.BeginTransactionAsync(ct);var load=await db.CargasMasivas.SingleOrDefaultAsync(x=>x.Id==id&&x.Usuario==User.Username()&&x.Estado=="PREVISUALIZADA",ct)??throw new KeyNotFoundException("Previsualización no encontrada o ya procesada.");var rows=(JsonSerializer.Deserialize<List<Dictionary<string,string>>>(load.FilasJson)??[]).Select(row=>new Dictionary<string,string>(row,StringComparer.OrdinalIgnoreCase)).ToList();var processed=0;if(load.Tipo=="llantas")processed=await ImportTires(rows,OriginalRowNumbers(load),User.AlcanceCentros(),ct);else if(load.Tipo=="vehiculos")processed=await ImportVehicles(rows,OriginalRowNumbers(load),User.AlcanceCentros(),ct);else throw new ValidacionException("Tipo de carga no soportado.");load.Estado="PROCESADA";load.FechaProcesamiento=DateTimeOffset.UtcNow;load.UsuarioModificacion=User.Username();await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);result=new(load.Id,processed,load.FilasConError);});return result;}
 [HttpGet("{id:guid}/errores")]public async Task<IActionResult> Errors(Guid id,CancellationToken ct){var load=await db.CargasMasivas.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id&&x.Usuario==User.Username(),ct)??throw new KeyNotFoundException();var errors=JsonSerializer.Deserialize<List<RowError>>(load.ErroresJson)??[];using var wb=new XLWorkbook();var ws=wb.AddWorksheet("Errores");ws.Cell(1,1).Value="Fila";ws.Cell(1,2).Value="Campo";ws.Cell(1,3).Value="Error";for(var i=0;i<errors.Count;i++){ws.Cell(i+2,1).Value=errors[i].Fila;ws.Cell(i+2,2).Value=errors[i].Campo;ws.Cell(i+2,3).Value=errors[i].Error;}using var ms=new MemoryStream();wb.SaveAs(ms);return File(ms.ToArray(),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",$"errores-{load.NombreArchivo}.xlsx");}
 private async Task<List<RowError>> Validate(string type,List<Dictionary<string,string>> rows,AlcanceCentros scope,CancellationToken ct)
 {
  if(type.Equals("llantas",StringComparison.OrdinalIgnoreCase))
   return (await ResolveTires(rows,Enumerable.Range(2,rows.Count).ToArray(),scope,ct)).Errors;

  return (await ResolveVehicles(rows,Enumerable.Range(2,rows.Count).ToArray(),scope,ct)).Errors;
 }

 private static int[] OriginalRowNumbers(CargaMasiva load)
 {
  // The stored errors also recover original numbers for previews created before this fix.
  var invalid=(JsonSerializer.Deserialize<List<RowError>>(load.ErroresJson)??[]).Select(x=>x.Fila).ToHashSet();
  return Enumerable.Range(2,load.TotalFilas).Where(n=>!invalid.Contains(n)).ToArray();
 }

 private async Task<(List<GuardarLlantaDto> Tires,List<RowError> Errors)> ResolveTires(
  List<Dictionary<string,string>> rows,IReadOnlyList<int> numbers,AlcanceCentros scope,CancellationToken ct)
 {
  var centers=await db.Centros.AsNoTracking().Where(x=>x.Activo).ToListAsync(ct);
  var brands=await db.Marcas.AsNoTracking().Where(x=>x.Activo).ToListAsync(ct);
  var references=await db.Referencias.AsNoTracking().Where(x=>x.Activo).ToListAsync(ct);
  var dimensions=await db.Dimensiones.AsNoTracking().Where(x=>x.Activo).ToListAsync(ct);
  var types=await db.TiposLlanta.AsNoTracking().Where(x=>x.Activo).ToListAsync(ct);
  var states=await db.EstadosLlanta.AsNoTracking().Where(x=>x.Activo).ToListAsync(ct);
  var existing=await db.Llantas.IgnoreQueryFilters().AsNoTracking().Select(x=>new{x.Codigo,x.Serial}).ToListAsync(ct);
  var codes=existing.Select(x=>x.Codigo.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
  var serials=existing.Select(x=>x.Serial.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
  var errors=new List<RowError>();
  var tires=new List<GuardarLlantaDto>();
  for(var i=0;i<rows.Count;i++)
  {
   var row=rows[i];var number=numbers[i];var before=errors.Count;
   string Value(string field)=>(row.GetValueOrDefault(field)??"").Trim();
   void Error(string field,string reason)=>errors.Add(new(number,field,$"{field} '{Value(field)}': {reason}"));
   T? Resolve<T>(IEnumerable<T> catalog,string field,string? missing=null) where T:CatalogoBase
   {
    var value=Value(field);
    var matches=catalog.Where(x=>string.Equals(x.Codigo.Trim(),value,StringComparison.OrdinalIgnoreCase)
     ||string.Equals(x.Nombre.Trim(),value,StringComparison.OrdinalIgnoreCase)).Take(2).ToList();
    if(matches.Count==1)return matches[0];
    Error(field,matches.Count==0?missing??"No existe en el catálogo activo.":"Coincide con varios registros activos; indique un código inequívoco.");
    return null;
   }
   var center=Resolve(centers,"Centro");
   if(center is not null&&!scope.Autoriza(center.Id))Error("Centro","Centro fuera del alcance autorizado.");
   var brand=Resolve(brands,"Marca");
   var reference=Resolve(references.Where(x=>brand is not null&&x.MarcaId==brand.Id),"Referencia",
    $"No existe una referencia activa con ese código o nombre perteneciente a la marca '{Value("Marca")}'.");
   var dimension=Resolve(dimensions,"Dimension");
   var type=Resolve(types,"TipoLlanta");
   var state=Resolve(states,"Estado");
   if(!codes.Add(Value("CodigoLlanta")))Error("CodigoLlanta","Identificador duplicado en el archivo o en el sistema.");
   if(!serials.Add(Value("Serial")))Error("Serial","Serial duplicado en el archivo o en el sistema.");
   var parsed=decimal.TryParse(Value("ProfundidadInicial"),NumberStyles.Number,CultureInfo.InvariantCulture,out var depth);
   if(!parsed)Error("ProfundidadInicial","Debe ser un número entre 0 y 100.");
   var dto=new GuardarLlantaDto{
    Codigo=Value("CodigoLlanta").ToUpperInvariant(),Serial=Value("Serial").ToUpperInvariant(),
    MarcaId=brand?.Id??Guid.Empty,ReferenciaId=reference?.Id??Guid.Empty,
    DimensionId=dimension?.Id??Guid.Empty,TipoLlantaId=type?.Id??Guid.Empty,
    EstadoLlantaId=state?.Id??Guid.Empty,CentroId=center?.Id??Guid.Empty,
    UbicacionActual=Value("Ubicacion"),ProfundidadInicial=depth
   };
   // Reuse the individual-create contract, including lengths and depth range.
   var validations=new List<ValidationResult>();
   Validator.TryValidateObject(dto,new ValidationContext(dto),validations,true);
   foreach(var validation in validations)
   {
    var member=validation.MemberNames.First();
    var field=member switch{nameof(GuardarLlantaDto.Codigo)=>"CodigoLlanta",nameof(GuardarLlantaDto.UbicacionActual)=>"Ubicacion",_=>member};
    Error(field,member==nameof(GuardarLlantaDto.ProfundidadInicial)?"Debe ser un número entre 0 y 100.":validation.ErrorMessage!);
   }
   if(errors.Count==before)tires.Add(dto);
  }
  return (tires,errors);
 }

 private async Task<int> ImportTires(List<Dictionary<string,string>> rows,IReadOnlyList<int> numbers,AlcanceCentros scope,CancellationToken ct)
 {
  // Resolve once with exactly the preview rules, then use those IDs without another lookup.
  var resolved=await ResolveTires(rows,numbers,scope,ct);
  if(resolved.Errors.Count>0)
  {
   var details=resolved.Errors.GroupBy(x=>$"Fila {x.Fila}: {x.Campo}")
    .ToDictionary(g=>g.Key,g=>g.Select(x=>x.Error).ToArray());
   var first=resolved.Errors[0];
   throw new ValidacionException($"Fila {first.Fila}: {first.Error}",details);
  }
  foreach(var dto in resolved.Tires)
   db.Llantas.Add(new Llanta(dto.Codigo,dto.Serial){
    MarcaId=dto.MarcaId,ReferenciaId=dto.ReferenciaId,DimensionId=dto.DimensionId,
    TipoLlantaId=dto.TipoLlantaId,EstadoLlantaId=dto.EstadoLlantaId,CentroId=dto.CentroId,
    UbicacionActual=dto.UbicacionActual,ProfundidadInicial=dto.ProfundidadInicial,UsuarioCreacion=User.Username()
   });
  // Confirm saves tires and load status together inside the existing transaction.
  return resolved.Tires.Count;
 }
 private async Task<(List<GuardarVehiculoDto> Vehicles,List<RowError> Errors)> ResolveVehicles(
  List<Dictionary<string,string>> rows,IReadOnlyList<int> numbers,AlcanceCentros scope,CancellationToken ct)
 {
  var centers=await db.Centros.AsNoTracking().Where(x=>x.Activo).ToListAsync(ct);
  var configurations=await db.ConfiguracionesVehiculo.AsNoTracking().Where(x=>x.Activo).ToListAsync(ct);
  // The unique internal number is reserved even by inactive vehicles.
  var existing=await db.Vehiculos.IgnoreQueryFilters().AsNoTracking().Select(x=>x.NumeroInterno).ToListAsync(ct);
  var internals=existing.Select(x=>x.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
  var vehicleModel=db.Model.FindEntityType(typeof(Vehiculo))!;
  var errors=new List<RowError>();
  var vehicles=new List<GuardarVehiculoDto>();
  for(var i=0;i<rows.Count;i++)
  {
   var row=rows[i];var number=numbers[i];var before=errors.Count;
   string Value(string field)=>(row.GetValueOrDefault(field)??"").Trim();
   void Error(string field,string reason)=>errors.Add(new(number,field,$"{field} '{Value(field)}': {reason}"));
   bool Matches(string candidate,string value)=>string.Equals(candidate.Trim(),value,StringComparison.OrdinalIgnoreCase);
   foreach(var field in VehicleColumns)
    if(Value(field).Length==0)Error(field,"Valor obligatorio.");

   // GuardarVehiculoDto has no validation attributes; use the actual EF string limits.
   foreach(var (field,property) in new[]{
    ("Interno",nameof(Vehiculo.NumeroInterno)),("Placa",nameof(Vehiculo.Placa)),
    ("TipoVehiculo",nameof(Vehiculo.Tipo)),("Estado",nameof(Vehiculo.Estado))})
   {
    var max=vehicleModel.FindProperty(property)!.GetMaxLength();
    if(max.HasValue&&Value(field).Length>max.Value)Error(field,$"Supera el máximo de {max.Value} caracteres.");
   }
   if(Value("Interno").Length>0&&!internals.Add(Value("Interno")))
    Error("Interno","Número interno duplicado en el archivo o en el sistema.");

   Centro? center=null;
   if(Value("Centro").Length>0)
   {
    var matches=centers.Where(x=>Matches(x.Codigo,Value("Centro"))||Matches(x.Nombre,Value("Centro"))).Take(2).ToList();
    if(matches.Count==0)Error("Centro","No existe un centro activo con ese código o nombre.");
    else if(matches.Count>1)Error("Centro","Coincide con varios centros activos; indique un código inequívoco.");
    else
    {
     center=matches[0];
     if(!scope.Autoriza(center.Id))Error("Centro","Centro fuera del alcance autorizado.");
    }
   }

   ConfiguracionVehiculo? configuration=null;
   if(Value("ConfiguracionEjes").Length>0)
   {
    var matches=configurations.Where(x=>Matches(x.Codigo,Value("ConfiguracionEjes"))||Matches(x.Nombre,Value("ConfiguracionEjes"))).Take(2).ToList();
    if(matches.Count==0)Error("ConfiguracionEjes","No existe una configuración activa con ese código o nombre.");
    else if(matches.Count>1)Error("ConfiguracionEjes","Coincide con varias configuraciones activas; indique un código inequívoco.");
    else
    {
     configuration=matches[0];
     if(Value("TipoVehiculo").Length>0&&!Matches(configuration.TipoVehiculo,Value("TipoVehiculo")))
      Error("ConfiguracionEjes",$"La configuración pertenece al tipo '{configuration.TipoVehiculo.Trim()}' y no a '{Value("TipoVehiculo")}'.");
    }
   }

   // Required by the bulk template, although the individual-create DTO permits null.
   // Use an explicit decimal point and no grouping or units, independent of server culture.
   var parsed=decimal.TryParse(Value("Kilometraje"),NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,
    CultureInfo.InvariantCulture,out var mileage);
   if(Value("Kilometraje").Length>0&&(!parsed||mileage<0))
    Error("Kilometraje","Debe ser un número mayor o igual a 0, sin unidades ni separadores de miles; use punto decimal.");
   else if(parsed&&mileage>9999999999999999.99m)
    Error("Kilometraje","Supera el máximo de 9999999999999999.99 permitido por la base de datos (decimal 18,2).");

   if(errors.Count==before)
    vehicles.Add(new(Value("Interno").ToUpperInvariant(),Value("Placa").ToUpperInvariant(),
     configuration!.TipoVehiculo.Trim(),center!.Id,configuration.Id,mileage,Value("Estado"),null));
  }
  return (vehicles,errors);
 }

 private async Task<int> ImportVehicles(List<Dictionary<string,string>> rows,IReadOnlyList<int> numbers,AlcanceCentros scope,CancellationToken ct)
 {
  var resolved=await ResolveVehicles(rows,numbers,scope,ct);
  if(resolved.Errors.Count>0)
  {
   var first=resolved.Errors[0];
   var details=resolved.Errors.GroupBy(x=>$"Fila {x.Fila}: {x.Campo}")
    .ToDictionary(g=>g.Key,g=>g.Select(x=>x.Error).ToArray());
   throw new ValidacionException($"Fila {first.Fila} - {first.Error}",details);
  }
  for(var i=0;i<resolved.Vehicles.Count;i++)
  {
   try
   {
    // Keep the individual creation path (including axle/position cloning), using resolved IDs.
    await vehiculos.CrearAsync(resolved.Vehicles[i],User.Username(),scope,ct);
   }
   catch(Exception ex) when(ex is ValidacionException or ConflictoException or UnauthorizedAccessException)
   {
    // A business rule can still change between resolution and insertion; preserve row context.
    var values=string.Join(", ",VehicleColumns.Select(field=>$"{field} '{rows[i].GetValueOrDefault(field)}'"));
    throw new ValidacionException($"Fila {numbers[i]} - {values}: {ex.Message}");
   }
  }
  return resolved.Vehicles.Count;
 }
 private static string[] Columns(string type)=>type.ToLowerInvariant() switch{"llantas"=>TireColumns,"vehiculos"=>VehicleColumns,_=>throw new ValidacionException("Tipo de plantilla no soportado.")};
 private static async Task<List<Dictionary<string,string>>> Read(IFormFile file,string ext,CancellationToken ct){await using var stream=file.OpenReadStream();if(ext==".xlsx"){using var wb=new XLWorkbook(stream);var ws=wb.Worksheets.First();var headers=ws.Row(1).CellsUsed().Select(x=>x.GetString().Trim()).ToList();return ws.RowsUsed().Skip(1).Select(row=>headers.Select((h,i)=>(h,v:row.Cell(i+1).GetFormattedString().Trim())).ToDictionary(x=>x.h,x=>x.v,StringComparer.OrdinalIgnoreCase)).ToList();}using var reader=new StreamReader(stream,Encoding.UTF8,true);var text=await reader.ReadToEndAsync(ct);using var parser=new TextFieldParser(new StringReader(text)){TextFieldType=FieldType.Delimited,HasFieldsEnclosedInQuotes=true};parser.SetDelimiters(",");var headersCsv=parser.ReadFields()??[];var result=new List<Dictionary<string,string>>();while(!parser.EndOfData){var values=parser.ReadFields()??[];result.Add(headersCsv.Select((h,i)=>(h:h.Trim(),v:i<values.Length?values[i].Trim():"")).ToDictionary(x=>x.h,x=>x.v,StringComparer.OrdinalIgnoreCase));}return result;}
 public sealed record RowError(int Fila,string Campo,string Error);public sealed record PreviewDto(Guid Id,int Total,int Validas,int ConError,IReadOnlyList<RowError> Errores);public sealed record ResultadoDto(Guid Id,int Procesadas,int Omitidas);
}
