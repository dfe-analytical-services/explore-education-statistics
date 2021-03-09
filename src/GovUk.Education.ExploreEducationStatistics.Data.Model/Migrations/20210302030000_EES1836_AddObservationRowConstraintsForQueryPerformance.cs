using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1836_AddObservationRowConstraintsForQueryPerformance : Migration
    {
        private const string MigrationId = "20210302030000";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, 
                $"{MigrationId}_AddObservationRowConstraintsForQueryPerformance.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
