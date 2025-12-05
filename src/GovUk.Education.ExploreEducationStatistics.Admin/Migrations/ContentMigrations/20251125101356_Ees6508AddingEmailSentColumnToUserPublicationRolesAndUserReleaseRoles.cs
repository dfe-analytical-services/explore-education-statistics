using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees6508AddingEmailSentColumnToUserPublicationRolesAndUserReleaseRoles : Migration
    {
        private const string MigrationId = "20251125101356";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmailSent",
                table: "UserReleaseRoles",
                type: "datetimeoffset",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmailSent",
                table: "UserPublicationRoles",
                type: "datetimeoffset",
                nullable: true
            );

            // Migrate 'EmailSent' values for existing UserPublicationRoles and UserReleaseRoles to be `Null` or `DateTimeOffset.MinValue`.
            migrationBuilder.SqlFromFile(
                MigrationConstants.ContentMigrationsPath,
                $"{MigrationId}_{nameof(Ees6508AddingEmailSentColumnToUserPublicationRolesAndUserReleaseRoles)}.sql"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "EmailSent", table: "UserReleaseRoles");

            migrationBuilder.DropColumn(name: "EmailSent", table: "UserPublicationRoles");
        }
    }
}
