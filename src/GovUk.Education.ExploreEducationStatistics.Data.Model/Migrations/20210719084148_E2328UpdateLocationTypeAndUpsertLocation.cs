using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class E2328UpdateLocationTypeAndUpsertLocation : Migration
    {
        public const string MigrationId = "20210719084148";
        private const string PreviousLocationTypeMigrationId = "20210512112804";
        private const string PreviousUpsertLocationMigrationId = "20210512112804";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE UpsertLocation");
            migrationBuilder.Sql("DROP TYPE LocationType");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_TableType_LocationType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_UpsertLocation.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE UpsertLocation");
            migrationBuilder.Sql("DROP TYPE LocationType");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousLocationTypeMigrationId}_TableType_LocationType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousUpsertLocationMigrationId}_Routine_UpsertLocation.sql");
        }
    }
}
