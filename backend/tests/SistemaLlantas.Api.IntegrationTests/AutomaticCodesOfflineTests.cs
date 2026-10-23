using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SistemaLlantas.Application.Common;
using System.Security.Claims;
using System.Text.Json;

namespace SistemaLlantas.Api.IntegrationTests;

// Never constructs TestApplicationFactory. All checks use EF metadata/SQL generation only.
public class AutomaticCodesOfflineTests
{
    [Fact] public async Task InvalidAbbreviationReturnsValidationErrorWithoutDatabaseAccess()
    {
        await using var db=new OfflineDesignFactory().CreateDbContext([]);
        var service=new SistemaLlantas.Infrastructure.Services.CatalogoService(db);
        await Assert.ThrowsAsync<ValidacionException>(()=>service.CrearAsync("marcas",
            new SistemaLlantas.Application.Catalogos.GuardarCatalogoDto("","Marca",Abreviatura:"ABC"),"operador",default));
        Assert.Equal(System.Data.ConnectionState.Closed,db.Database.GetDbConnection().State);
        Assert.Empty(db.ChangeTracker.Entries());
    }
    [Theory]
    [InlineData(nameof(Llanta.Serial))]
    [InlineData(nameof(Llanta.ProfundidadInicial))]
    [InlineData(nameof(Llanta.IdentificadorOriginal))]
    public void UnknownValuesRemainNullable(string property)
    {
        using var db=new OfflineDesignFactory().CreateDbContext([]);
        Assert.True(db.Model.FindEntityType(typeof(Llanta))!.FindProperty(property)!.IsNullable);
    }
    [Fact] public void UnknownMountDateAndUniqueActiveAssignmentsArePreserved()
    {
        using var db=new OfflineDesignFactory().CreateDbContext([]);
        var type=db.Model.FindEntityType(typeof(AsignacionLlantaPosicion))!;
        Assert.True(type.FindProperty(nameof(AsignacionLlantaPosicion.FechaInicio))!.IsNullable);
        Assert.Equal(2,type.GetIndexes().Count(x=>x.IsUnique && x.GetFilter()!=null));
    }
    [Fact] public void OriginalIdentifierAndAbbreviationHaveUniqueFilteredIndexes()
    {
        using var db=new OfflineDesignFactory().CreateDbContext([]);
        foreach(var (entity,property) in new[]{(typeof(Marca),nameof(Marca.Abreviatura)),(typeof(Llanta),nameof(Llanta.IdentificadorOriginal)),(typeof(Llanta),nameof(Llanta.Serial))})
            Assert.Contains(db.Model.FindEntityType(entity)!.GetIndexes(),x=>x.IsUnique&&x.Properties.SingleOrDefault()?.Name==property&&x.GetFilter()!=null);
    }
    [Fact] public void ScriptIsIncrementalRestrictedToDevelopmentAndDoesNotSeed()
    {
        using var db=new OfflineDesignFactory().CreateDbContext([]);
        var sql=db.GetService<IMigrator>().GenerateScript("20261006162816_AddDisposicionSistemaVerde","20261022151207_AddAutomaticCodesAndImportedMounts");
        Assert.Contains("DB_NAME() <> N'GDLLSQLDLLO'",sql);
        Assert.Contains("IX_Llanta_SSerial",sql);
        Assert.DoesNotContain("INSERT INTO [TBL_",sql);
        Assert.DoesNotContain("DROP TABLE",sql);
        Assert.DoesNotContain("sp_rename",sql);
    }
    [Fact] public void TemplatesDoNotRequireGeneratedCodeAndMountAddsPosition()
    {
        Assert.DoesNotContain("CodigoLlanta",CargaMasivaController.InventarioColumns);
        Assert.Contains("IdentificadorOriginal",CargaMasivaController.InventarioColumns);
        Assert.Contains("Posicion",CargaMasivaController.MontajeColumns);
        Assert.Contains("FechaMontaje",CargaMasivaController.MontajeColumns);
        Assert.Contains("KilometrajeMontaje",CargaMasivaController.MontajeColumns);
    }
    [Theory] [InlineData("otro",false)] [InlineData("operador",true)]
    public async Task PreviewTokenRejectsOtherUserOrExpiryBeforeDatabaseAccess(string user,bool expired)
    {
        await using var db=new OfflineDesignFactory().CreateDbContext([]);
        var provider=new EphemeralDataProtectionProvider();
        using var services=new ServiceCollection().AddSingleton<IDataProtectionProvider>(provider).BuildServiceProvider();
        var c=new CargaMasivaController(db,null!){ControllerContext=new ControllerContext{HttpContext=new DefaultHttpContext{RequestServices=services,User=new ClaimsPrincipal(new ClaimsIdentity([new Claim("username","operador")],"test"))}}};
        var ticket=new CargaMasivaController.Ticket(Guid.NewGuid(),user,"montaje","hash",DateTimeOffset.UtcNow.AddMinutes(expired?-1:30));
        var token=provider.CreateProtector("GLLD.ImportacionLlantas.v2").Protect(JsonSerializer.Serialize(ticket));
        await Assert.ThrowsAsync<ValidacionException>(()=>c.ConfirmLlantas(token,null!,default));
        Assert.Equal(System.Data.ConnectionState.Closed,db.Database.GetDbConnection().State);
    }
}
