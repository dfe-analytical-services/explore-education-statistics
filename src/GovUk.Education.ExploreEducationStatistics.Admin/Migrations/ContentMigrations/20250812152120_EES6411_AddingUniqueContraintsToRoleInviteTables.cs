#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
public partial class EES6411_AddingUniqueContraintsToRoleInviteTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_UserReleaseInvites_ReleaseVersionId",
            table: "UserReleaseInvites");

        migrationBuilder.DropIndex(
            name: "IX_UserPublicationInvites_PublicationId",
            table: "UserPublicationInvites");

        migrationBuilder.AlterColumn<string>(
            name: "Role",
            table: "UserReleaseInvites",
            type: "nvarchar(450)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "Email",
            table: "UserReleaseInvites",
            type: "nvarchar(450)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "Role",
            table: "UserPublicationInvites",
            type: "nvarchar(450)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "Email",
            table: "UserPublicationInvites",
            type: "nvarchar(450)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.CreateIndex(
            name: "IX_UserReleaseInvites_ReleaseVersionId_Email_Role",
            table: "UserReleaseInvites",
            columns: new[] { "ReleaseVersionId", "Email", "Role" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserPublicationInvites_PublicationId_Email_Role",
            table: "UserPublicationInvites",
            columns: new[] { "PublicationId", "Email", "Role" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_UserReleaseInvites_ReleaseVersionId_Email_Role",
            table: "UserReleaseInvites");

        migrationBuilder.DropIndex(
            name: "IX_UserPublicationInvites_PublicationId_Email_Role",
            table: "UserPublicationInvites");

        migrationBuilder.AlterColumn<string>(
            name: "Role",
            table: "UserReleaseInvites",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)");

        migrationBuilder.AlterColumn<string>(
            name: "Email",
            table: "UserReleaseInvites",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)");

        migrationBuilder.AlterColumn<string>(
            name: "Role",
            table: "UserPublicationInvites",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)");

        migrationBuilder.AlterColumn<string>(
            name: "Email",
            table: "UserPublicationInvites",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)");

        migrationBuilder.CreateIndex(
            name: "IX_UserReleaseInvites_ReleaseVersionId",
            table: "UserReleaseInvites",
            column: "ReleaseVersionId");

        migrationBuilder.CreateIndex(
            name: "IX_UserPublicationInvites_PublicationId",
            table: "UserPublicationInvites",
            column: "PublicationId");
    }
}
