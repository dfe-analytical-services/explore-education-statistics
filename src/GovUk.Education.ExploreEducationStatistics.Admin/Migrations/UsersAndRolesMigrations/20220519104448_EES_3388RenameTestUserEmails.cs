using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.UsersAndRolesMigrations
{
    public partial class EES_3388RenameTestUserEmails : Migration
    {
        public const string MigrationId = "20220519104448";
        private const string RenameTestUserEmailsUpsertSqlFile = $"{MigrationId}_EES_3388RenameTestUserEmails.sql";
        private const string RenameTestUserEmailsDownSqlFile = $"{MigrationId}_EES_3388RenameTestUserEmails-down.sql";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(UsersAndRolesMigrationsPath, RenameTestUserEmailsUpsertSqlFile);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(UsersAndRolesMigrationsPath, RenameTestUserEmailsDownSqlFile);

        }
    }
}
