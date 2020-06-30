using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1087_Disable_Observation_Constraints : Migration
    {
        private const string MigrationsPath = "Migrations";
        private const string MigrationId = "20200629095712";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_DisableObservationConstraints.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_EnableObservationConstraints.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TYPE dbo.DisableObservationConstraints");
            migrationBuilder.Sql("DROP TYPE dbo.EnableObservationConstraints");
        }
    }
}
