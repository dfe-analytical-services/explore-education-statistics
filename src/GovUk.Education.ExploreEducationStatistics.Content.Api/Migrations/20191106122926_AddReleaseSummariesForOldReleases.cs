using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class AddReleaseSummariesForOldReleases : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var file = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Migrations/20191106122926_AddReleaseSummariesForOldReleasesUp.sql"
                );
            
            migrationBuilder.Sql(File.ReadAllText(file));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }

    }
}
