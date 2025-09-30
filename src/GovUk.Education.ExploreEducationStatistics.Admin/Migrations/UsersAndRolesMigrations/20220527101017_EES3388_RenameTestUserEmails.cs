using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.UsersAndRolesMigrations;

// ReSharper disable once InconsistentNaming
public partial class EES3388_RenameTestUserEmails : Migration
{
    private const string MigrationId = "20220527101017";
    private const string RenameTestUserEmailsUpsertSqlFile = $"{MigrationId}_EES3388_RenameTestUserEmails_upsert.sql";
    private const string RenameTestUserEmailsDownSqlFile = $"{MigrationId}_EES3388_RenameTestUserEmails_down.sql";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.SqlFromFile(UsersAndRolesMigrationsPath, RenameTestUserEmailsUpsertSqlFile);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.SqlFromFile(UsersAndRolesMigrationsPath, RenameTestUserEmailsDownSqlFile);
    }
}
