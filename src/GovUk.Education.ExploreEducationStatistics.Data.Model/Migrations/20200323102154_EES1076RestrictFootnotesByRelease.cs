using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1076RestrictFootnotesByRelease : Migration
    {
        private const string MigrationId = "20200323102154";
        private const string PreviousVersionMigrationId = "20200302162939";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_FilteredFootnotes.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to the version in the previous migration 20200302162939_Routine_FilteredFootnotes.sql
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousVersionMigrationId}_Routine_FilteredFootnotes.sql");
        }
    }
}
