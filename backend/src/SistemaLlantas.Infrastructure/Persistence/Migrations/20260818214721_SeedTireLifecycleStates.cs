using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedTireLifecycleStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @Estados TABLE (Codigo nvarchar(30), Nombre nvarchar(150), PermiteMontaje bit, EsDisposicionFinal bit);
                INSERT INTO @Estados VALUES
                ('DISPONIBLE','Disponible',1,0),('MONTADA','Montada',0,0),('EN_INSPECCION','En inspección',0,0),
                ('PEND_APROBACION','Pendiente de aprobación',0,0),('EN_REPARACION','En reparación',0,0),
                ('EN_REENCAUCHE','En reencauche',0,0),('EN_TRASLADO','En traslado',0,0),
                ('PEND_DISPOSICION','Pendiente disposición',0,0),('DISPOSICION_FINAL','Disposición final',0,1),
                ('BLOQUEADA','Bloqueada',0,0),('INACTIVA','Inactiva',0,0);
                INSERT INTO TBL_EstadoLlanta (Id,Codigo,Nombre,PermiteMontaje,EsDisposicionFinal,FechaCreacion,UsuarioCreacion,Activo)
                SELECT NEWID(),e.Codigo,e.Nombre,e.PermiteMontaje,e.EsDisposicionFinal,SYSDATETIMEOFFSET(),'migration-phase3',1
                FROM @Estados e WHERE NOT EXISTS (SELECT 1 FROM TBL_EstadoLlanta x WHERE x.Codigo=e.Codigo);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM TBL_EstadoLlanta WHERE UsuarioCreacion='migration-phase3' AND Codigo IN ('DISPONIBLE','MONTADA','EN_INSPECCION','PEND_APROBACION','EN_REPARACION','EN_REENCAUCHE','EN_TRASLADO','PEND_DISPOSICION','DISPOSICION_FINAL','BLOQUEADA','INACTIVA')");
        }
    }
}
