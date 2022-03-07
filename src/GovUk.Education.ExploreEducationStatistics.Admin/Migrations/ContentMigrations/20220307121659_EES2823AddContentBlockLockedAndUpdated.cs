using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES2823AddContentBlockLockedAndUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Locked",
                table: "ContentBlock",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LockedById",
                table: "ContentBlock",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "ContentBlock",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentBlock_LockedById",
                table: "ContentBlock",
                column: "LockedById");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentBlock_Users_LockedById",
                table: "ContentBlock",
                column: "LockedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentBlock_Users_LockedById",
                table: "ContentBlock");

            migrationBuilder.DropIndex(
                name: "IX_ContentBlock_LockedById",
                table: "ContentBlock");

            migrationBuilder.DropColumn(
                name: "Locked",
                table: "ContentBlock");

            migrationBuilder.DropColumn(
                name: "LockedById",
                table: "ContentBlock");

            migrationBuilder.DropColumn(
                name: "Updated",
                table: "ContentBlock");
        }
    }
}
