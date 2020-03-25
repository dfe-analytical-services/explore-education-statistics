using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1417AlterReleaseType : Migration
    {
        private const string MigrationId = "20200304163715";
        private const string DropAndCreateReleaseProcedureMigrationId = "20200303144410";
        private const string PreviousVersionMigrationId = "20200217131418";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // It's necessary to drop the procedure even though we aren't changing it
            migrationBuilder.Sql("DROP PROCEDURE dbo.DropAndCreateRelease");
            
            migrationBuilder.Sql("DROP TYPE dbo.ReleaseType");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Type_ReleaseType.sql");
            
            // Restore the procedure
            migrationBuilder.SqlFromFile(MigrationsPath, $"{DropAndCreateReleaseProcedureMigrationId}_Routine_DropAndCreateRelease.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.DropAndCreateRelease");
            migrationBuilder.Sql("DROP TYPE dbo.ReleaseType");
            
            // Revert to the version in the previous migration 20200217131418_Type_ReleaseType.sql
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousVersionMigrationId}_Type_ReleaseType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{DropAndCreateReleaseProcedureMigrationId}_Routine_DropAndCreateRelease.sql");
        }
    }
}
