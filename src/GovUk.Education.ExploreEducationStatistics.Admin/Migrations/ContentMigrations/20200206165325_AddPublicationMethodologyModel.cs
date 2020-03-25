using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class AddPublicationMethodologyModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MethodologyUrl",
                table: "Publications");

            migrationBuilder.AddColumn<string>(
                name: "ExternalMethodology_Title",
                table: "Publications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalMethodology_Url",
                table: "Publications",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalMethodology_Title",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "ExternalMethodology_Url",
                table: "Publications");

            migrationBuilder.AddColumn<string>(
                name: "MethodologyUrl",
                table: "Publications",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
