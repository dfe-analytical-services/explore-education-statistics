using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES977DifferentiateTitlesAndSlugs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Make changes corresponding to ContentDb migration 20200614095817_EES977DifferentiateTitlesAndSlugs
            // if the Publications exist (true in statistics data files have been uploaded)
            migrationBuilder.Sql(
                "UPDATE dbo.Publication SET Slug = 'multi-academy-trust-performance-measures-at-ks2', Title = 'Multi-academy trust performance measures at key stage 2' WHERE Id = 'eab51107-4ef0-4926-8f8b-c8bd7f5a21d5'");
            migrationBuilder.Sql(
                "UPDATE dbo.Publication SET Slug = 'multi-academy-trust-performance-measures-at-ks4', Title = 'Multi-academy trust performance measures at key stage 4' WHERE Id = '1d0e4263-3d70-433e-bd95-f29754db5888'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE dbo.Publication SET Slug = 'multi-academy-trust-performance-measures', Title = 'Multi-academy trust performance measures' WHERE Id = 'eab51107-4ef0-4926-8f8b-c8bd7f5a21d5'");
            migrationBuilder.Sql(
                "UPDATE dbo.Publication SET Slug = 'multi-academy-trust-performance-measures', Title = 'Multi-academy trust performance measures' WHERE Id = '1d0e4263-3d70-433e-bd95-f29754db5888'");
        }
    }
}
