using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

[ExcludeFromCodeCoverage]
public partial class EES4976_Update_Multiple_Boundaries_Geometry_Data : Migration
{
    private const string MigrationId = "20240311090051";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_BoundaryLevels.sql");

        migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Countries_GeometryData.sql");
        migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Regions_GeometryData.sql");
        migrationBuilder.SqlFromFile(
            MigrationsPath,
            $"{MigrationId}_LocalAuthorities_GeometryData.sql"
        );
        migrationBuilder.SqlFromFile(
            MigrationsPath,
            $"{MigrationId}_LocalAuthorityDistricts_GeometryData.sql"
        );
        migrationBuilder.SqlFromFile(
            MigrationsPath,
            $"{MigrationId}_LocalEnterprisePartnerships_GeometryData.sql"
        );
        migrationBuilder.SqlFromFile(
            MigrationsPath,
            $"{MigrationId}_ParliamentaryConstituencies_GeometryData.sql"
        );
        migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Wards_GeometryData.sql");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM dbo.geometry WHERE boundary_level_id BETWEEN 14 AND 20");
        migrationBuilder.Sql("DELETE FROM dbo.BoundaryLevel WHERE Id BETWEEN 14 AND 20");
    }
}
