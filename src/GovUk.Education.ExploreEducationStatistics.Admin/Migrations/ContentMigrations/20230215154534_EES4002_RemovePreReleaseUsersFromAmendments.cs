using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES4002_RemovePreReleaseUsersFromAmendments : Migration
    {
        private const string MigrationId = "20230215154534";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationConstants.ContentMigrationsPath,
                $"{MigrationId}_EES4002_RemovePreReleaseUsersFromAmendments.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
