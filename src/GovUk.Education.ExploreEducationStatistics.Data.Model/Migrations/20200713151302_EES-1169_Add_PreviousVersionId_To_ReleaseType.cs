using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1169_Add_PreviousVersionId_To_ReleaseType : Migration
    {
        private const string MigrationId = "20200713151302";
        private const string PreviousVersionMigrationId = "20200703122015";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.DropAndCreateRelease");
            migrationBuilder.Sql("DROP TYPE dbo.ReleaseType");
            
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Type_ReleaseType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_DropAndCreateRelease.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.DropAndCreateRelease");
            migrationBuilder.Sql("DROP TYPE dbo.ReleaseType");
            
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Type_Previous_ReleaseType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousVersionMigrationId}_Routine_DropAndCreateRelease.sql");
        }
    }
}
