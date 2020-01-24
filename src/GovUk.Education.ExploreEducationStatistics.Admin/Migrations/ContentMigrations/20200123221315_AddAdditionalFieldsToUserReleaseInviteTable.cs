using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class AddAdditionalFieldsToUserReleaseInviteTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Accepted",
                table: "UserReleaseInvites",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "UserReleaseInvites",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "UserReleaseInvites",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
            
            migrationBuilder.CreateIndex(
                name: "IX_UserReleaseInvites_CreatedById",
                table: "UserReleaseInvites",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_UserReleaseInvites_Users_CreatedById",
                table: "UserReleaseInvites",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserReleaseInvites_Users_CreatedById",
                table: "UserReleaseInvites");

            migrationBuilder.DropIndex(
                name: "IX_UserReleaseInvites_CreatedById",
                table: "UserReleaseInvites");

            migrationBuilder.DropColumn(
                name: "Accepted",
                table: "UserReleaseInvites");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "UserReleaseInvites");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "UserReleaseInvites");
        }
    }
}
