using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees6511MigratingUserResourceInvitesToRoles : Migration
    {
        private const string MigrationId = "20260113152512";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_UserReleaseRoles_UserId", table: "UserReleaseRoles");

            migrationBuilder.DropIndex(name: "IX_UserPublicationRoles_UserId", table: "UserPublicationRoles");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "UserReleaseRoles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "UserReleaseRoles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "UserPublicationRoles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "UserPublicationRoles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserReleaseRoles_UserId_ReleaseVersionId_Role",
                table: "UserReleaseRoles",
                columns: new[] { "UserId", "ReleaseVersionId", "Role" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserPublicationRoles_UserId_PublicationId_Role",
                table: "UserPublicationRoles",
                columns: new[] { "UserId", "PublicationId", "Role" },
                unique: true
            );

            // Migrate existing UserPublicationInvites/UserReleaseInvites to corresponding UserPublicationRoles/UserReleaseRoles
            migrationBuilder.SqlFromFile(
                MigrationConstants.ContentMigrationsPath,
                $"{MigrationId}_{nameof(Ees6511MigratingUserResourceInvitesToRoles)}.sql"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserReleaseRoles_UserId_ReleaseVersionId_Role",
                table: "UserReleaseRoles"
            );

            migrationBuilder.DropIndex(
                name: "IX_UserPublicationRoles_UserId_PublicationId_Role",
                table: "UserPublicationRoles"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "UserReleaseRoles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "UserReleaseRoles",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "UserPublicationRoles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "UserPublicationRoles",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2"
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserReleaseRoles_UserId",
                table: "UserReleaseRoles",
                column: "UserId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserPublicationRoles_UserId",
                table: "UserPublicationRoles",
                column: "UserId"
            );
        }
    }
}
