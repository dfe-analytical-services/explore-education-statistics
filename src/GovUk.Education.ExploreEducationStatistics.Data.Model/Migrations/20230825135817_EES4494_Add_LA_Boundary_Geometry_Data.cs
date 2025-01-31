using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

[ExcludeFromCodeCoverage]
public partial class EES4494_Add_LA_Boundary_Geometry_Data : Migration
{
    private const string MigrationId = "20230825135817";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_BoundaryLevel.sql");
        migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_GeometryData.sql");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM dbo.geometry WHERE boundary_level_id = 12");
        migrationBuilder.Sql("DELETE FROM dbo.BoundaryLevel WHERE Id = 12");
    }
}
