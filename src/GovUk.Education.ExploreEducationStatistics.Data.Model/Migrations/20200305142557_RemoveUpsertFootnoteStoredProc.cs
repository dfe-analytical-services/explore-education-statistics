using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class RemoveUpsertFootnoteStoredProc : Migration
    {
        private const string PreviousVersionMigrationId = "20200103101609";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertFootnote");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to the version in the previous migration 20200103101609_Routine_UpsertFootnote
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousVersionMigrationId}_Routine_UpsertFootnote.sql");
        }
    }
}
