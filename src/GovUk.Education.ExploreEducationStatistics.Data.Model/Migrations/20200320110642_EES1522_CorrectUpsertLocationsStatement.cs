using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1522_CorrectUpsertLocationsStatement : Migration
    {
        private const string MigrationId = "20200320110642";
        private const string PreviousVersionMigrationId = "20200308154804";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Restore the procedure
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_UpsertLocation.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to the version in the previous migration 20200308154804_Routine_UpsertLocation.sql
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousVersionMigrationId}_Routine_UpsertLocation.sql");
        }
    }
}
