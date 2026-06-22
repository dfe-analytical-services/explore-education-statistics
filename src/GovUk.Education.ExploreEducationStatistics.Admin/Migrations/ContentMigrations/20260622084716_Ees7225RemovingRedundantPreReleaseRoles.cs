using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7225RemovingRedundantPreReleaseRoles : Migration
    {
        private const string MigrationId = "20260622084716";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove any redundant pre-release roles that have been created for users who already have a
            // publication-level role for the same release.
            migrationBuilder.SqlFromFile(
                MigrationConstants.ContentMigrationsPath,
                $"{MigrationId}_{nameof(Ees7225RemovingRedundantPreReleaseRoles)}.sql"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
