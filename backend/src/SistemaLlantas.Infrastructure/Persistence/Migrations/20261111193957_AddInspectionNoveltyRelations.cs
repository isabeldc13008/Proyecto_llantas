using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionNoveltyRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBL_CausaRecomendacionInspeccion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CausaLlantaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecomendacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_CausaRecomendacionInspeccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_CausaRecomendacionInspeccion_TBL_CausaLlanta_CausaLlantaId",
                        column: x => x.CausaLlantaId,
                        principalSchema: "dbo",
                        principalTable: "TBL_CausaLlanta",
                        principalColumn: "GId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TBL_CausaRecomendacionInspeccion_TBL_RecomendacionInspeccion_RecomendacionId",
                        column: x => x.RecomendacionId,
                        principalSchema: "dbo",
                        principalTable: "TBL_RecomendacionInspeccion",
                        principalColumn: "GId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TBL_CondicionCausaLlanta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CondicionLlantaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CausaLlantaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_CondicionCausaLlanta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_CondicionCausaLlanta_TBL_CausaLlanta_CausaLlantaId",
                        column: x => x.CausaLlantaId,
                        principalSchema: "dbo",
                        principalTable: "TBL_CausaLlanta",
                        principalColumn: "GId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TBL_CondicionCausaLlanta_TBL_CondicionLlanta_CondicionLlantaId",
                        column: x => x.CondicionLlantaId,
                        principalSchema: "dbo",
                        principalTable: "TBL_CondicionLlanta",
                        principalColumn: "GId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_CausaRecomendacionInspeccion_CausaLlantaId_RecomendacionId",
                table: "TBL_CausaRecomendacionInspeccion",
                columns: new[] { "CausaLlantaId", "RecomendacionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_CausaRecomendacionInspeccion_RecomendacionId",
                table: "TBL_CausaRecomendacionInspeccion",
                column: "RecomendacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_CondicionCausaLlanta_CausaLlantaId",
                table: "TBL_CondicionCausaLlanta",
                column: "CausaLlantaId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_CondicionCausaLlanta_CondicionLlantaId_CausaLlantaId",
                table: "TBL_CondicionCausaLlanta",
                columns: new[] { "CondicionLlantaId", "CausaLlantaId" },
                unique: true);

            migrationBuilder.Sql("""
                DECLARE @now datetimeoffset = SYSDATETIMEOFFSET();
                DECLARE @user nvarchar(max) = N'migration-inspection-relations';
                INSERT INTO dbo.TBL_CondicionLlanta (GId,BRequiereCausa,DFechaCreacion,SUsuarioCreacion,BActivo,SCodigo,SNombre)
                SELECT NEWID(),1,@now,@user,1,v.Codigo,v.Nombre FROM (VALUES
                    (N'DANIO_COSTADO',N'Daño en el costado'),(N'DANIO_HOMBRO',N'Daño en el hombro'),
                    (N'DANIO_BANDA',N'Daño en la banda de rodamiento'),(N'DANIO_CARCASA',N'Daño en la carcasa'),
                    (N'DANIO_PESTANA',N'Daño en la pestaña'),(N'DANIO_MECANICO',N'Daño mecánico'),
                    (N'DANIO_NO_REPARABLE',N'Daño no reparable'),(N'DESGASTE_EXCESIVO',N'Desgaste excesivo'),
                    (N'FALLA_REPARACION',N'Falla de reparación'),(N'FIN_VIDA',N'Fin de vida útil'),
                    (N'IMPACTO_GOLPE',N'Impacto o golpe'),(N'PERFORACIONES_MULTIPLES',N'Perforaciones múltiples'),
                    (N'SEPARACION_COMPONENTES',N'Separación de componentes')
                ) v(Codigo,Nombre) WHERE NOT EXISTS (SELECT 1 FROM dbo.TBL_CondicionLlanta x WHERE x.SCodigo=v.Codigo);
                INSERT INTO dbo.TBL_CausaLlanta (GId,DFechaCreacion,SUsuarioCreacion,BActivo,SCodigo,SNombre)
                SELECT NEWID(),@now,@user,1,v.Codigo,v.Nombre FROM (VALUES
                    (N'IMPACTO_COSTADO',N'Impacto en el costado'),(N'INFILTRACION_COSTADO',N'Infiltración en el costado'),(N'FILTRACION_COSTADO',N'Filtración por el costado'),(N'SEPARACION_COSTADO',N'Separación en el costado'),(N'EXPOSICION_CINTURONES_COSTADO',N'Exposición de cinturones en el costado'),
                    (N'IMPACTO_HOMBRO',N'Impacto en el hombro'),(N'DESGARRE_HOMBRO',N'Desgarre en el hombro'),
                    (N'IMPACTO_BANDA',N'Impacto en la banda de rodamiento'),(N'DESPRENDIMIENTO_BANDA',N'Desprendimiento de la banda'),(N'MULTIPLES_DANIOS_BANDA',N'Múltiples daños en la banda'),(N'DESGARRE_BANDA',N'Desgarre de banda'),(N'FRENO_LOCALIZADO',N'Freno localizado'),(N'SEPARACION_CINTURONES',N'Separación de cinturones'),
                    (N'FALLA_CARCASA',N'Falla de la carcasa'),(N'FATIGA_CARCASA',N'Fatiga de carcasa'),(N'SEPARACION_ZONA_VOLTEO',N'Separación en la zona de volteo'),
                    (N'PESTANA_QUEMADA',N'Pestaña quemada'),(N'DESGARRE_PESTANA',N'Desgarre de pestaña'),(N'ASENTAMIENTO_RIN',N'Asentamiento de rin'),
                    (N'DANIO_MECANICO',N'Daño mecánico'),(N'RODADA_BAJA_PRESION',N'Rodada a baja presión'),(N'DESGASTE_EXCESIVO',N'Desgaste excesivo'),(N'FALLA_REPARACION',N'Falla de reparación'),(N'DANIOS_NO_REPARABLES',N'Daños considerados no reparables'),(N'RECHAZO_PLANTA_PELADA',N'Rechazo de planta - pelada'),(N'LLANTA_LIVIANA_FVU',N'Llanta liviana - FVU'),
                    (N'FIN_VIDA',N'Fin de vida'),(N'MULTIPLES_PERFORACIONES',N'Múltiples perforaciones')
                ) v(Codigo,Nombre) WHERE NOT EXISTS (SELECT 1 FROM dbo.TBL_CausaLlanta x WHERE x.SCodigo=v.Codigo);
                INSERT INTO dbo.TBL_RecomendacionInspeccion (GId,BEsCandidataReencauche,DFechaCreacion,SUsuarioCreacion,BActivo,SCodigo,SNombre)
                SELECT NEWID(),0,@now,@user,1,v.Codigo,v.Nombre FROM (VALUES
                    (N'ANALISIS_TECNICO',N'Enviar a análisis técnico'),(N'DISPOSICION_FINAL',N'Enviar a disposición final'),(N'EVALUAR_CARCASA',N'Evaluar estado de carcasa'),(N'EVALUAR_REPARABILIDAD',N'Evaluar reparabilidad'),(N'RETIRAR_SEGURIDAD',N'Retirar de operación por seguridad')
                ) v(Codigo,Nombre) WHERE NOT EXISTS (SELECT 1 FROM dbo.TBL_RecomendacionInspeccion x WHERE x.SCodigo=v.Codigo);
                INSERT INTO dbo.TBL_CondicionCausaLlanta (GId,CondicionLlantaId,CausaLlantaId,DFechaCreacion,SUsuarioCreacion,BActivo)
                SELECT NEWID(),c.GId,a.GId,@now,@user,1 FROM (VALUES
                    (N'DANIO_COSTADO',N'IMPACTO_COSTADO'),(N'DANIO_COSTADO',N'INFILTRACION_COSTADO'),(N'DANIO_COSTADO',N'FILTRACION_COSTADO'),(N'DANIO_COSTADO',N'SEPARACION_COSTADO'),(N'DANIO_COSTADO',N'EXPOSICION_CINTURONES_COSTADO'),
                    (N'DANIO_HOMBRO',N'IMPACTO_HOMBRO'),(N'DANIO_HOMBRO',N'DESGARRE_HOMBRO'),(N'DANIO_BANDA',N'IMPACTO_BANDA'),(N'DANIO_BANDA',N'DESPRENDIMIENTO_BANDA'),(N'DANIO_BANDA',N'MULTIPLES_DANIOS_BANDA'),(N'DANIO_BANDA',N'DESGARRE_BANDA'),(N'DANIO_BANDA',N'FRENO_LOCALIZADO'),(N'DANIO_BANDA',N'SEPARACION_CINTURONES'),
                    (N'DANIO_CARCASA',N'FALLA_CARCASA'),(N'DANIO_CARCASA',N'FATIGA_CARCASA'),(N'DANIO_CARCASA',N'SEPARACION_ZONA_VOLTEO'),(N'DANIO_PESTANA',N'PESTANA_QUEMADA'),(N'DANIO_PESTANA',N'DESGARRE_PESTANA'),(N'DANIO_PESTANA',N'ASENTAMIENTO_RIN'),
                    (N'DANIO_MECANICO',N'DANIO_MECANICO'),(N'DANIO_MECANICO',N'RODADA_BAJA_PRESION'),(N'DANIO_NO_REPARABLE',N'DANIOS_NO_REPARABLES'),(N'DANIO_NO_REPARABLE',N'RECHAZO_PLANTA_PELADA'),(N'DANIO_NO_REPARABLE',N'LLANTA_LIVIANA_FVU'),
                    (N'DESGASTE_EXCESIVO',N'DESGASTE_EXCESIVO'),(N'DESGASTE_EXCESIVO',N'RODADA_BAJA_PRESION'),(N'DESGASTE_EXCESIVO',N'FRENO_LOCALIZADO'),(N'FALLA_REPARACION',N'FALLA_REPARACION'),(N'FIN_VIDA',N'FIN_VIDA'),(N'FIN_VIDA',N'LLANTA_LIVIANA_FVU'),
                    (N'IMPACTO_GOLPE',N'IMPACTO_COSTADO'),(N'IMPACTO_GOLPE',N'IMPACTO_HOMBRO'),(N'IMPACTO_GOLPE',N'IMPACTO_BANDA'),(N'PERFORACIONES_MULTIPLES',N'MULTIPLES_PERFORACIONES'),(N'SEPARACION_COMPONENTES',N'SEPARACION_COSTADO'),(N'SEPARACION_COMPONENTES',N'SEPARACION_ZONA_VOLTEO'),(N'SEPARACION_COMPONENTES',N'SEPARACION_CINTURONES'),(N'SEPARACION_COMPONENTES',N'DESPRENDIMIENTO_BANDA')
                ) v(Condicion,Causa) JOIN dbo.TBL_CondicionLlanta c ON c.SCodigo=v.Condicion JOIN dbo.TBL_CausaLlanta a ON a.SCodigo=v.Causa
                WHERE NOT EXISTS (SELECT 1 FROM dbo.TBL_CondicionCausaLlanta x WHERE x.CondicionLlantaId=c.GId AND x.CausaLlantaId=a.GId);
                INSERT INTO dbo.TBL_CausaRecomendacionInspeccion (GId,CausaLlantaId,RecomendacionId,DFechaCreacion,SUsuarioCreacion,BActivo)
                SELECT NEWID(),a.GId,r.GId,@now,@user,1 FROM dbo.TBL_CausaLlanta a CROSS JOIN dbo.TBL_RecomendacionInspeccion r
                WHERE a.BActivo=1 AND r.BActivo=1 AND r.SCodigo IN (N'ANALISIS_TECNICO',N'EVALUAR_CARCASA',N'EVALUAR_REPARABILIDAD',N'RETIRAR_SEGURIDAD')
                  AND NOT EXISTS (SELECT 1 FROM dbo.TBL_CausaRecomendacionInspeccion x WHERE x.CausaLlantaId=a.GId AND x.RecomendacionId=r.GId);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBL_CausaRecomendacionInspeccion");

            migrationBuilder.DropTable(
                name: "TBL_CondicionCausaLlanta");
        }
    }
}
