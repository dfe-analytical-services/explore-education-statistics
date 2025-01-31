using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES2686RenameReleaseSubjectMetaGuidanceToDataGuidance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MetaGuidance",
                newName: "DataGuidance",
                table: "ReleaseSubject");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DataGuidance",
                newName: "MetaGuidance",
                table: "ReleaseSubject");
        }
    }
}
