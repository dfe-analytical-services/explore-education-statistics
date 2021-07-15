using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES2222_AddCascadeDeleteToPublicationMethodologyIfPublicationRemoved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PublicationMethodologies_Publications_PublicationId",
                table: "PublicationMethodologies");

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationMethodologies_Publications_PublicationId",
                table: "PublicationMethodologies",
                column: "PublicationId",
                principalTable: "Publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PublicationMethodologies_Publications_PublicationId",
                table: "PublicationMethodologies");

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationMethodologies_Publications_PublicationId",
                table: "PublicationMethodologies",
                column: "PublicationId",
                principalTable: "Publications",
                principalColumn: "Id");
        }
    }
}
