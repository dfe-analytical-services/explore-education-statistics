using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class AddNewLADBoundaryLevelApril2019 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new LAD boundary level data
            ExecuteFile(migrationBuilder, "Migrations/20190729134031_AddNewLADBoundaryLevelApril2019.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop new LAD boundary level
            migrationBuilder.Sql("DELETE FROM BoundaryLevel WHERE Id=8;");
        }
    }
}
