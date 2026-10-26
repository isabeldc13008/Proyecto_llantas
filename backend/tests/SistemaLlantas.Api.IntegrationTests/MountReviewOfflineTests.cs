using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
namespace SistemaLlantas.Api.IntegrationTests;
// Metadata and authorization contract only: never creates a host or opens a connection.
public class MountReviewOfflineTests
{
 [Fact] public void MigrationOnlyAddsOptionalAuditColumns(){using var db=new OfflineDesignFactory().CreateDbContext([]);var sql=db.GetService<IMigrator>().GenerateScript("20261022151207_AddAutomaticCodesAndImportedMounts","20261026141651_AddMountPositionDecisions");foreach(var name in new[]{"DFechaEjecucion","SEjecutor","SDestinoSolicitadoJson","SHistorialRevisionJson"})Assert.Contains(name,sql);Assert.DoesNotContain("DROP",sql);Assert.DoesNotContain("UPDATE [",sql);Assert.DoesNotContain("CREATE TABLE",sql);Assert.Equal(System.Data.ConnectionState.Closed,db.Database.GetDbConnection().State);}
 [Fact] public void ReviewRetainsOptimisticConcurrency(){using var db=new OfflineDesignFactory().CreateDbContext([]);var type=db.Model.FindEntityType(typeof(SolicitudOperacion))!;Assert.True(type.FindProperty(nameof(SolicitudOperacion.RowVersion))!.IsConcurrencyToken);foreach(var prop in new[]{nameof(SolicitudOperacion.HistorialRevisionJson),nameof(SolicitudOperacion.DestinoSolicitadoJson),nameof(SolicitudOperacion.Ejecutor),nameof(SolicitudOperacion.FechaEjecucion)})Assert.True(type.FindProperty(prop)!.IsNullable);}
 [Theory][InlineData(nameof(OperacionesController.DecidirPosicion),"Operaciones.Aprobar")][InlineData(nameof(OperacionesController.CorregirPosicion),"Operaciones.Solicitar")][InlineData(nameof(OperacionesController.EjecutarPosiciones),"Operaciones.Montar")][InlineData(nameof(OperacionesController.CambiarProgramacion),"Operaciones.Montar")]
 public void ActionsRequireExistingPolicies(string method,string policy)=>Assert.Contains(typeof(OperacionesController).GetMethod(method)!.GetCustomAttributes(typeof(AuthorizeAttribute),true).Cast<AuthorizeAttribute>(),a=>a.Policy==policy);
}
