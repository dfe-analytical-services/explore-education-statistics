using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class ChangePublicationExternalMethodologyToTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalMethodology_Title",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "ExternalMethodology_Url",
                table: "Publications");

            migrationBuilder.CreateTable(
                name: "ExternalMethodology",
                columns: table => new
                {
                    PublicationId = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalMethodology", x => x.PublicationId);
                    table.ForeignKey(
                        name: "FK_ExternalMethodology_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalMethodology");

            migrationBuilder.AddColumn<string>(
                name: "ExternalMethodology_Title",
                table: "Publications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalMethodology_Url",
                table: "Publications",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
