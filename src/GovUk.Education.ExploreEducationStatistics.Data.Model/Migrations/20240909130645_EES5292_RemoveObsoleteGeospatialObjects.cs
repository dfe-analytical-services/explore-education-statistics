using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

/// <inheritdoc />
public partial class EES5292_RemoveObsoleteGeospatialObjects : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("geometry");
        migrationBuilder.DropTable("geometry_columns");
        migrationBuilder.DropTable("spatial_ref_sys");
        migrationBuilder.Sql("DROP FUNCTION geometry2json");
        migrationBuilder.Sql("DROP VIEW geojson");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
