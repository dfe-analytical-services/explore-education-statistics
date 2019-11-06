using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class AddReleaseSummariesForOldReleases : Migration
    {
        private const string MigrationsPath = "Migrations/ContentMigrations/";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var file = Path.Combine(
                Directory.GetCurrentDirectory(),
                MigrationsPath + "20191106122926_AddReleaseSummariesForOldReleasesUp.sql"
                );
            
            migrationBuilder.Sql(File.ReadAllText(file));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
