#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class EES6370_AddAdditionalReportingFieldsToReleasePublishingFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // These new columns are nullable right now until a manual
            // migration endpoint is run to populate them.
            //
            // EES-6416 will handle making them non-nullable when all
            // rows are migrated.
            migrationBuilder.AddColumn<string>(
                name: "PublicationTitle",
                table: "ReleasePublishingFeedback",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReleaseTitle",
                table: "ReleasePublishingFeedback",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicationTitle",
                table: "ReleasePublishingFeedback");

            migrationBuilder.DropColumn(
                name: "ReleaseTitle",
                table: "ReleasePublishingFeedback");
        }
    }
}
