using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES4668_AddReleasePublicationAssociation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PublicationId",
                table: "Releases",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Releases_PublicationId",
                table: "Releases",
                column: "PublicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Releases_Publications_PublicationId",
                table: "Releases",
                column: "PublicationId",
                principalTable: "Publications",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Releases_Publications_PublicationId",
                table: "Releases");

            migrationBuilder.DropIndex(
                name: "IX_Releases_PublicationId",
                table: "Releases");

            migrationBuilder.DropColumn(
                name: "PublicationId",
                table: "Releases");
        }
    }
}
