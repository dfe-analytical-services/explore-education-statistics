using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1921UpdateRoutineUpsertLocation : Migration
    {
        private const string MigrationId = "20210309112852";
        private const string PreviousLocationTypeMigrationId = "20200308154804";
        private const string PreviousUpsertLocationMigrationId = "20200320110642";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertLocation");
            migrationBuilder.Sql("DROP TYPE dbo.LocationType");

            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Type_LocationType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_UpsertLocation.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertLocation");
            migrationBuilder.Sql("DROP TYPE dbo.LocationType");

            migrationBuilder.SqlFromFile(MigrationsPath, 
                $"{PreviousLocationTypeMigrationId}_Type_LocationType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath,
                $"{PreviousUpsertLocationMigrationId}_Routine_UpsertLocation.sql");
        }
    }
}
