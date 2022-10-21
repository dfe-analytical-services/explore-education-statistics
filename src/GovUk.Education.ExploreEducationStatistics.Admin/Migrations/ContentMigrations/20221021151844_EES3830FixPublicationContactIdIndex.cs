using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES3830FixPublicationContactIdIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Publications_ContactId",
                table: "Publications");

            migrationBuilder.CreateIndex(
                name: "IX_Publications_ContactId",
                table: "Publications",
                column: "ContactId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Publications_ContactId",
                table: "Publications");

            migrationBuilder.CreateIndex(
                name: "IX_Publications_ContactId",
                table: "Publications",
                column: "ContactId");
        }
    }
}
