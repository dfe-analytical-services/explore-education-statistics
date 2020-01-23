using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations
{
    public partial class AddRolesSelectionAndLinksToReleasesToInviteProcess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "UserInvites",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "RoleId1",
                table: "UserInvites",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserInvites_RoleId1",
                table: "UserInvites",
                column: "RoleId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserInvites_AspNetRoles_RoleId1",
                table: "UserInvites",
                column: "RoleId1",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserInvites_AspNetRoles_RoleId1",
                table: "UserInvites");

            migrationBuilder.DropIndex(
                name: "IX_UserInvites_RoleId1",
                table: "UserInvites");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "UserInvites");

            migrationBuilder.DropColumn(
                name: "RoleId1",
                table: "UserInvites");
        }
    }
}
