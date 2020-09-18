using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1089_Rebuild_Indexes : Migration
    {
        private const string MigrationId = "20200910135829";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_RebuildIndexes.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.RebuildIndexes");
        }
    }
}
