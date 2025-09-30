#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

// ReSharper disable once InconsistentNaming
public partial class EES4770_AddPublicationReleaseSeriesColumn : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ReleaseSeries",
            table: "Publications",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "[]"); // to ensure pages still load until we use the migration endpoint
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ReleaseSeries",
            table: "Publications");
    }
}
