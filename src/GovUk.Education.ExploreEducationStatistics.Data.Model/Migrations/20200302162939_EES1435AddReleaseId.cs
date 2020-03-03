using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1435AddReleaseId : Migration
    {
        private const string MigrationsPath = "Migrations";
        private const string MigrationId = "20200302162939";
        private const string PreviousVersionMigrationId = "20190819131247";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_FilteredFootnotes.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to the version in the previous migration 20190819131247_Routine_FilteredFootnotes.sql
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousVersionMigrationId}_Routine_FilteredFootnotes.sql");
        }
    }
}
