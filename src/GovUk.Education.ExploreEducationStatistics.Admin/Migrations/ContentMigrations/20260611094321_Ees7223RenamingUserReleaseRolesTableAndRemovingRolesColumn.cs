using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7223RenamingUserReleaseRolesTableAndRemovingRolesColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserReleaseRoles_ReleaseVersions_ReleaseVersionId",
                table: "UserReleaseRoles"
            );

            migrationBuilder.DropForeignKey(name: "FK_UserReleaseRoles_Users_CreatedById", table: "UserReleaseRoles");

            migrationBuilder.DropForeignKey(name: "FK_UserReleaseRoles_Users_UserId", table: "UserReleaseRoles");

            migrationBuilder.DropPrimaryKey(name: "PK_UserReleaseRoles", table: "UserReleaseRoles");

            migrationBuilder.DropColumn(name: "Role", table: "UserReleaseRoles");

            migrationBuilder.RenameTable(name: "UserReleaseRoles", newName: "UserPreReleaseRoles");

            migrationBuilder.RenameIndex(
                name: "IX_UserReleaseRoles_CreatedById",
                table: "UserPreReleaseRoles",
                newName: "IX_UserPreReleaseRoles_CreatedById"
            );

            migrationBuilder.RenameIndex(
                name: "IX_UserReleaseRoles_ReleaseVersionId",
                table: "UserPreReleaseRoles",
                newName: "IX_UserPreReleaseRoles_ReleaseVersionId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_UserReleaseRoles_UserId_ReleaseVersionId",
                table: "UserPreReleaseRoles",
                newName: "IX_UserPreReleaseRoles_UserId_ReleaseVersionId"
            );

            migrationBuilder.AddPrimaryKey(name: "PK_UserPreReleaseRoles", table: "UserPreReleaseRoles", column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreReleaseRoles_ReleaseVersions_ReleaseVersionId",
                table: "UserPreReleaseRoles",
                column: "ReleaseVersionId",
                principalTable: "ReleaseVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreReleaseRoles_Users_CreatedById",
                table: "UserPreReleaseRoles",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreReleaseRoles_Users_UserId",
                table: "UserPreReleaseRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPreReleaseRoles_ReleaseVersions_ReleaseVersionId",
                table: "UserPreReleaseRoles"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_UserPreReleaseRoles_Users_CreatedById",
                table: "UserPreReleaseRoles"
            );

            migrationBuilder.DropForeignKey(name: "FK_UserPreReleaseRoles_Users_UserId", table: "UserPreReleaseRoles");

            migrationBuilder.DropPrimaryKey(name: "PK_UserPreReleaseRoles", table: "UserPreReleaseRoles");

            migrationBuilder.RenameTable(name: "UserPreReleaseRoles", newName: "UserReleaseRoles");

            migrationBuilder.RenameIndex(
                name: "IX_UserPreReleaseRoles_CreatedById",
                table: "UserReleaseRoles",
                newName: "IX_UserReleaseRoles_CreatedById"
            );

            migrationBuilder.RenameIndex(
                name: "IX_UserPreReleaseRoles_ReleaseVersionId",
                table: "UserReleaseRoles",
                newName: "IX_UserReleaseRoles_ReleaseVersionId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_UserPreReleaseRoles_UserId_ReleaseVersionId",
                table: "UserReleaseRoles",
                newName: "IX_UserReleaseRoles_UserId_ReleaseVersionId"
            );

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "UserReleaseRoles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddPrimaryKey(name: "PK_UserReleaseRoles", table: "UserReleaseRoles", column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserReleaseRoles_ReleaseVersions_ReleaseVersionId",
                table: "UserReleaseRoles",
                column: "ReleaseVersionId",
                principalTable: "ReleaseVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_UserReleaseRoles_Users_CreatedById",
                table: "UserReleaseRoles",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_UserReleaseRoles_Users_UserId",
                table: "UserReleaseRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
