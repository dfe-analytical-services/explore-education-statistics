using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES968AddNDeletedAndDeletedByIdToUserReleaseRolesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "UserReleaseRoles",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "UserReleaseRoles",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Deleted",
                table: "UserReleaseRoles",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedById",
                table: "UserReleaseRoles",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserReleaseRoles_CreatedById",
                table: "UserReleaseRoles",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserReleaseRoles_DeletedById",
                table: "UserReleaseRoles",
                column: "DeletedById");

            migrationBuilder.AddForeignKey(
                name: "FK_UserReleaseRoles_Users_CreatedById",
                table: "UserReleaseRoles",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserReleaseRoles_Users_DeletedById",
                table: "UserReleaseRoles",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserReleaseRoles_Users_CreatedById",
                table: "UserReleaseRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserReleaseRoles_Users_DeletedById",
                table: "UserReleaseRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserReleaseRoles_CreatedById",
                table: "UserReleaseRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserReleaseRoles_DeletedById",
                table: "UserReleaseRoles");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "UserReleaseRoles");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "UserReleaseRoles");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "UserReleaseRoles");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "UserReleaseRoles");
        }
    }
}
