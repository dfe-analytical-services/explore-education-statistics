using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
public partial class EES5738_CreateDataSetFileGeographicLevelsTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DataSetFileVersionGeographicLevels",
            columns: table => new
            {
                DataSetFileVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                GeographicLevel = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DataSetFileVersionGeographicLevels", x => new { x.DataSetFileVersionId, x.GeographicLevel });
                table.ForeignKey(
                    name: "FK_DataSetFileVersionGeographicLevels_Files_DataSetFileVersionId",
                    column: x => x.DataSetFileVersionId,
                    principalTable: "Files",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.Sql("GRANT SELECT ON dbo.DataSetFileVersionGeographicLevels TO [content];");
        migrationBuilder.Sql("GRANT INSERT ON dbo.DataSetFileVersionGeographicLevels TO [importer];");

        migrationBuilder.Sql("GRANT SELECT ON dbo.DataSetFileVersionGeographicLevels TO [data];");
        migrationBuilder.Sql("GRANT SELECT ON dbo.DataSetFileVersionGeographicLevels TO [publisher];");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("REVOKE SELECT ON dbo.DataSetFileGeographicLevels TO [content];");
        migrationBuilder.Sql("REVOKE INSERT ON dbo.DataSetFileGeographicLevels TO [importer];");

        migrationBuilder.Sql("REVOKE SELECT ON dbo.DataSetFileGeographicLevels TO [data];");
        migrationBuilder.Sql("REVOKE SELECT ON dbo.DataSetFileGeographicLevels TO [publisher];");

        migrationBuilder.DropTable(
            name: "DataSetFileVersionGeographicLevels");
    }
}
