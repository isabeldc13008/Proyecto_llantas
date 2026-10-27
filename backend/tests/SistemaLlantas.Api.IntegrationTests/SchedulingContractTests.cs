using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.IntegrationTests;

// Metadata only: never opens a connection or runs migrations.
public sealed class SchedulingContractTests
{
 [Fact] public void EscriturasExigenPermisoAdministrar()
 {
  foreach(var name in new[]{"Crear","Masiva","Actualizar","Cancelar","Eliminar"})
   Assert.Contains(typeof(ProgramacionController).GetMethod(name)!.GetCustomAttributes<AuthorizeAttribute>(),a=>a.Policy=="Programacion.Administrar");
 }
 [Fact] public void ModeloCompatibleConAddScheduledMountSets()
 {
  using var db=new LlantasDbContext(new DbContextOptionsBuilder<LlantasDbContext>().UseSqlServer("Server=localhost;Database=MetadataOnly;Integrated Security=true").Options);
  var activity=db.Model.FindEntityType(typeof(ActividadProgramada))!;
  var index=Assert.Single(activity.GetIndexes(),i=>i.GetDatabaseName()=="IX_ActividadProgramada_TrabajoPosicion");
  Assert.True(index.IsUnique);
  Assert.Equal(new[]{"TecnicoUsuarioId","VehiculoId","TipoActividad","FechaProgramada","PosicionVehiculoId"},index.Properties.Select(p=>p.Name));
  Assert.Equal("([BActivo]=(1) AND [NEstado]<>(4) AND [GTecnicoUsuarioId] IS NOT NULL)",index.GetFilter());
  var request=db.Model.FindEntityType(typeof(SolicitudOperacion))!;
  var group=request.FindProperty(nameof(SolicitudOperacion.GrupoOperacionId))!;
  Assert.Equal("GGrupoOperacionId",group.GetColumnName(StoreObjectIdentifier.Table("TBL_SolicitudOperacion","dbo")));
  Assert.True(group.IsNullable);
 }
}
