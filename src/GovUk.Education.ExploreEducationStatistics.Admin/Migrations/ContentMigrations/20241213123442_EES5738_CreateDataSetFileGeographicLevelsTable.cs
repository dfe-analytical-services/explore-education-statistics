using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class EES5738_CreateDataSetFileGeographicLevelsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataSetFileGeographicLevels",
                columns: table => new
                {
                    DataSetFileVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GeographicLevel = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetFileGeographicLevels", x => new { x.DataSetFileVersionId, x.GeographicLevel });
                    table.ForeignKey(
                        name: "FK_DataSetFileGeographicLevels_Files_DataSetFileVersionId",
                        column: x => x.DataSetFileVersionId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("GRANT SELECT ON dbo.DataSetFileGeographicLevels TO [content];");
            migrationBuilder.Sql("GRANT INSERT ON dbo.DataSetFileGeographicLevels TO [importer];");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("REVOKE SELECT ON dbo.DataSetFileGeographicLevels TO [content];");
            migrationBuilder.Sql("REVOKE INSERT ON dbo.DataSetFileGeographicLevels TO [importer];");
            migrationBuilder.DropTable(
                name: "DataSetFileGeographicLevels");
        }
    }
}
