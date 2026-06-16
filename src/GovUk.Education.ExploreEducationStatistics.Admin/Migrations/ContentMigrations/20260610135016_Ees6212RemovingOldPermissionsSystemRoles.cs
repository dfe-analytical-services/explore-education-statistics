using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees6212RemovingOldPermissionsSystemRoles : Migration
    {
        private const string MigrationId = "20260610135016";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove all OLD permissions system roles:
            // - Publication `Allower`
            // - Publication `Owner`
            // - Release `Approver`
            // - Release `Contributor`
            migrationBuilder.SqlFromFile(
                MigrationConstants.ContentMigrationsPath,
                $"{MigrationId}_{nameof(Ees6212RemovingOldPermissionsSystemRoles)}.sql"
            );

            migrationBuilder.DropIndex(
                name: "IX_UserReleaseRoles_UserId_ReleaseVersionId_Role",
                table: "UserReleaseRoles"
            );

            migrationBuilder.DropIndex(
                name: "IX_UserPublicationRoles_UserId_PublicationId_Role",
                table: "UserPublicationRoles"
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserReleaseRoles_UserId_ReleaseVersionId",
                table: "UserReleaseRoles",
                columns: new[] { "UserId", "ReleaseVersionId" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserPublicationRoles_UserId_PublicationId",
                table: "UserPublicationRoles",
                columns: new[] { "UserId", "PublicationId" },
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_UserReleaseRoles_UserId_ReleaseVersionId", table: "UserReleaseRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserPublicationRoles_UserId_PublicationId",
                table: "UserPublicationRoles"
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
        }
    }
}
