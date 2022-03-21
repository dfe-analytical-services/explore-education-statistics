using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class EES3065_DropRoutineUpateLocationGeographicLevel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the temporary procedure which was used to set Location.GeographicLevel = Observation.GeographicLevel
            // for Locations referenced by Observations.
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpdateLocationGeographicLevel");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, 
                $"{EES2776_AddGeographicLevelToLocation.MigrationId}_Routine_UpdateLocationGeographicLevel.sql");
        }
    }
}
