using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees6511AddingUniqueIndexToUserResourceRoles : Migration
    {
        private const string MigrationId = "20251208181950";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_UserReleaseRoles_UserId", table: "UserReleaseRoles");

            migrationBuilder.DropIndex(name: "IX_UserPublicationRoles_UserId", table: "UserPublicationRoles");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "UserReleaseRoles",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "UserPublicationRoles",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
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
                $"{MigrationId}_{nameof(Ees6511AddingUniqueIndexToUserResourceRoles)}.sql"
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
                oldType: "nvarchar(450)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "UserPublicationRoles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)"
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
