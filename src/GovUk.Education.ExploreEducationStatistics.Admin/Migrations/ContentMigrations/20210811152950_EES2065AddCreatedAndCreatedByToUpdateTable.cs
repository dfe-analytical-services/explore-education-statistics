using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES2065AddCreatedAndCreatedByToUpdateTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Update",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Update",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Update_CreatedById",
                table: "Update",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Update_Users_CreatedById",
                table: "Update",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Update_Users_CreatedById",
                table: "Update");

            migrationBuilder.DropIndex(
                name: "IX_Update_CreatedById",
                table: "Update");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Update");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Update");
        }
    }
}
