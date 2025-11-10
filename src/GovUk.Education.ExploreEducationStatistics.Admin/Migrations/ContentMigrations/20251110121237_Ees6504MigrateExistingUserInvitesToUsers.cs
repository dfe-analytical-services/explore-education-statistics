using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees6504MigrateExistingUserInvitesToUsers : Migration
    {
        private const string MigrationId = "20251110121237";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migrate existing UNACCEPTED UserInvites to Users
            migrationBuilder.SqlFromFile(
                MigrationConstants.ContentMigrationsPath,
                $"{MigrationId}_{nameof(Ees6504MigrateExistingUserInvitesToUsers)}.sql"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
