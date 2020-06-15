using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES701RemoveReleaseIdFromCharts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE dbo.ContentBlock SET DataBlock_Charts = JSON_MODIFY(JSON_QUERY(DataBlock_Charts,'$'), '$[0].ReleaseId', NULL) WHERE 1=1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE dbo.ContentBlock SET DataBlock_Charts = JSON_MODIFY(DataBlock_Charts, '$[0].ReleaseId', '') WHERE 1=1");
        }
    }
}