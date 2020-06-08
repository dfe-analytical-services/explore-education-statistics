using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES970ChangePublicationTitle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Make changes corresponding to ContentDb migration 20200608113801_EES970ChangePublicationTitle
            // if the Publication exists (true in the statistics db but false in the public-statistics db)
            migrationBuilder.Sql("UPDATE dbo.Publication SET Slug = 'secondary-and-primary-school-applications-and-offers', Title = 'Secondary and primary school applications' WHERE Id = '66c8e9db-8bf2-4b0b-b094-cfab25c20b05'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE dbo.Publication SET Slug = 'secondary-and-primary-schools-applications-and-offers', Title = 'Secondary and primary schools applications and offers' WHERE Id = '66c8e9db-8bf2-4b0b-b094-cfab25c20b05'");
        }
    }
}
