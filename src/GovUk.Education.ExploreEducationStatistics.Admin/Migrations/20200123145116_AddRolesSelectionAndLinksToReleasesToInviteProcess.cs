using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations
{
    public partial class AddRolesSelectionAndLinksToReleasesToInviteProcess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoleId",
                table: "UserInvites",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_UserInvites_RoleId",
                table: "UserInvites",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserInvites_AspNetRoles_RoleId",
                table: "UserInvites",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserInvites_AspNetRoles_RoleId",
                table: "UserInvites");

            migrationBuilder.DropIndex(
                name: "IX_UserInvites_RoleId",
                table: "UserInvites");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "UserInvites");
        }
    }
}
