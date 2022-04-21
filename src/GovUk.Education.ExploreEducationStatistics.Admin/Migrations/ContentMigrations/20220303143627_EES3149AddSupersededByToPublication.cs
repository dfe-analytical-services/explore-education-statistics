using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES3149AddSupersededByToPublication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SupersededById",
                table: "Publications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Publications_SupersededById",
                table: "Publications",
                column: "SupersededById");

            migrationBuilder.AddForeignKey(
                name: "FK_Publications_Publications_SupersededById",
                table: "Publications",
                column: "SupersededById",
                principalTable: "Publications",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Publications_Publications_SupersededById",
                table: "Publications");

            migrationBuilder.DropIndex(
                name: "IX_Publications_SupersededById",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "SupersededById",
                table: "Publications");
        }
    }
}
