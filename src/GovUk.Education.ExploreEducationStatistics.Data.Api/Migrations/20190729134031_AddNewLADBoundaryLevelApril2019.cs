using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class AddNewLADBoundaryLevelApril2019 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new LAD boundary level data
            var file = Path.Combine(Directory.GetCurrentDirectory(), "Migrations/20190729134031_AddNewLADBoundaryLevelApril2019.sql");
            migrationBuilder.Sql(File.ReadAllText(file));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop new LAD boundary level
            migrationBuilder.Sql("DELETE FROM BoundaryLevel WHERE Id=8;");
        }
    }
}
