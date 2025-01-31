using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES2352_RemoveMethodologyFromPublication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Publications_Methodologies_MethodologyId",
                table: "Publications");

            migrationBuilder.DropIndex(
                name: "IX_Publications_MethodologyId",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "MethodologyId",
                table: "Publications");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MethodologyId",
                table: "Publications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Publications_MethodologyId",
                table: "Publications",
                column: "MethodologyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Publications_Methodologies_MethodologyId",
                table: "Publications",
                column: "MethodologyId",
                principalTable: "Methodologies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
