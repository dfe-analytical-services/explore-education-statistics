using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Hosting;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations;

/// <inheritdoc />
public partial class EES5329_GrantPublicDataReadWriteRolePrivileges : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        if (migrationBuilder.IsEnvironment(Environments.Production))
        {
            // Grant privileges to the PublicDataReadWriteRole group role for existing objects in the public schema
            migrationBuilder.Sql(
                $"GRANT SELECT, INSERT, UPDATE, DELETE, TRUNCATE, REFERENCES ON ALL TABLES IN SCHEMA public TO {PublicDataDbContext.PublicDataReadWriteRole}");
            migrationBuilder.Sql(
                $"GRANT SELECT, UPDATE ON ALL SEQUENCES IN SCHEMA public TO {PublicDataDbContext.PublicDataReadWriteRole}");

            // Grant privileges to the PublicDataReadWriteRole group role for objects in the public schema
            // subsequently created by this applications user role.
            migrationBuilder.Sql(
                $"ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE, TRUNCATE, REFERENCES ON TABLES TO {PublicDataDbContext.PublicDataReadWriteRole}");
            migrationBuilder.Sql(
                $"ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, UPDATE ON SEQUENCES TO {PublicDataDbContext.PublicDataReadWriteRole}");
        }
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
