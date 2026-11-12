using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;
namespace SistemaLlantas.Api.IntegrationTests;

public class ImportRowsOfflineTests
{
    private sealed class Scenario
    {
        public Centro Center=new(){Codigo="C",Nombre="Centro"};
        public Marca Brand=new(){Codigo="MC_100",Nombre="Marca",Abreviatura="GO"};
        public Dimension Dimension=new(){Codigo="DM_100"};
        public TipoLlanta Type=new(){Codigo="TL_100"};
        public EstadoLlanta State=new(){Codigo="MONTADA"};
        public Referencia Reference;
        public PosicionVehiculo Position;
        public Scenario()
        {
            Reference=new(){Codigo="RF_MC_100_D200",MarcaId=Brand.Id};
            Position=new(){Codigo="1",EjeVehiculo=new(){Vehiculo=new(){NumeroInterno="10",Placa="AAA111",CentroId=Center.Id}}};
        }
        public Dictionary<string,string> Row(string id="00001")=>new(StringComparer.OrdinalIgnoreCase){["IdentificadorOriginal"]=id,["Marca"]=Brand.Codigo,["Referencia"]=Reference.Codigo,["Dimension"]=Dimension.Codigo,["TipoLlanta"]=Type.Codigo,["Centro"]=Center.Codigo,["Estado"]=State.Codigo,["Placa"]="AAA111",["Posicion"]="1"};
        public (List<CargaMasivaController.ImportRow> Rows,List<CargaMasivaController.RowError> Errors) Validate(List<Dictionary<string,string>> rows,string mode="montaje",string[]? existing=null,Guid[]? occupied=null,AlcanceCentros? scope=null)=>
            CargaMasivaController.ValidateImportRows(rows,mode,[Center],[Brand],[Reference],[Dimension],[Type],[State],existing??[],[],[Position],occupied??[],scope??new(true,[]),"operador");
    }
    [Fact] public void PreviewDoesNotMutatePositionOrAllocateCodesAndPreservesUnknownData()
    {
        var s=new Scenario();var result=s.Validate([s.Row()]);Assert.Empty(result.Errors);
        var row=Assert.Single(result.Rows);Assert.Null(s.Position.LlantaActualId);Assert.Null(row.Date);Assert.Null(row.Km);
        Assert.Null(row.Tire.ProfundidadInicial);Assert.Null(row.Tire.Serial);Assert.Equal("00001",row.Tire.IdentificadorOriginal);
        Assert.Equal("PENDIENTE",row.Tire.Codigo);Assert.Equal(s.State.Id,row.Tire.EstadoLlantaId);Assert.Equal(s.Position,row.Position);
    }
    [Theory] [InlineData("Placa","NOEXISTE")] [InlineData("Interno","OTRO")] [InlineData("Posicion","99")]
    public void MissingVehicleOrMismatchingPositionIsReported(string field,string value)
    {
        var s=new Scenario();var row=s.Row();row[field]=value;var result=s.Validate([row]);Assert.Empty(result.Rows);Assert.Contains(result.Errors,e=>e.Fila==2&&e.Campo=="Posicion");
    }
    [Fact] public void OccupiedPositionIsRejectedWithoutMutation()
    {
        var s=new Scenario();var result=s.Validate([s.Row()],occupied:[s.Position.Id]);Assert.Empty(result.Rows);Assert.Contains(result.Errors,e=>e.Campo=="Posicion");Assert.Null(s.Position.LlantaActualId);
    }
    [Fact] public void RepeatedPositionInSameBatchIsRejected()
    {
        var s=new Scenario();var result=s.Validate([s.Row("1"),s.Row("2")]);Assert.Contains(result.Errors,e=>e.Fila==3&&e.Campo=="Posicion");
    }
    [Fact] public void DuplicateOriginalIsRejectedEvenWithNoSerial()
    {
        var s=new Scenario();var result=s.Validate([s.Row()],existing:["00001"]);Assert.Empty(result.Rows);Assert.Contains(result.Errors,e=>e.Campo=="IdentificadorOriginal");
    }
    [Fact] public void MissingAbbreviationCannotCreateATire()
    {
        var s=new Scenario();s.Brand.Abreviatura=null;var result=s.Validate([s.Row()]);Assert.Empty(result.Rows);Assert.Contains(result.Errors,e=>e.Campo=="Marca");
    }
    [Fact] public void InventoryDoesNotCreateMountAndRespectsCenterScope()
    {
        var s=new Scenario();s.State.Codigo="DISPONIBLE";var valid=s.Validate([s.Row()],"inventario");Assert.Empty(valid.Errors);Assert.Null(Assert.Single(valid.Rows).Position);
        var denied=s.Validate([s.Row()],"inventario",scope:new(false,[]));Assert.Empty(denied.Rows);Assert.Contains(denied.Errors,e=>e.Campo=="Centro");
    }
    [Theory] [InlineData("ProfundidadInterior","-1")] [InlineData("ProfundidadInicial","101")] [InlineData("KilometrajeMontaje","1,200")] [InlineData("FechaMontaje","2099-01-01T00:00:00+00:00")]
    public void InvalidMeasurementIsReportedByColumn(string field,string value)
    {
        var s=new Scenario();var row=s.Row();row[field]=value;var result=s.Validate([row]);Assert.Empty(result.Rows);Assert.Contains(result.Errors,e=>e.Campo==field);
    }
    [Fact] public void SuppliedMountDateAndKmAndLifeArePreserved()
    {
        var s=new Scenario();var row=s.Row();row["FechaMontaje"]="2026-01-01T12:00:00-05:00";row["KilometrajeMontaje"]="123.45";row["Vida"]="3";
        var result=s.Validate([row]);Assert.Empty(result.Errors);var found=Assert.Single(result.Rows);Assert.Equal(123.45m,found.Km);Assert.Equal(TimeSpan.FromHours(-5),found.Date!.Value.Offset);Assert.Equal(2,found.Tire.NumeroReencauches);
    }
}
