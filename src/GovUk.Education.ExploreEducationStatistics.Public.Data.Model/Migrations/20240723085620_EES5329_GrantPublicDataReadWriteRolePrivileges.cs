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
            // Grant privileges to the 'public_data_read_write' group role for existing objects in the public schema
            // and those subsequently created by this applications user role. Membership of the role will be granted to
            // other application and indvidual user roles who require read and write privileges on public schema objects.

            // In Azure `public_data_read_write` should be created manually before this migration is run using:
            // `CREATE ROLE public_data_read_write WITH NOLOGIN`.

            // Local development environments skip this since the Docker entrypoint script handles role creation
            // and privilege grants.

            // A check ensures the role exists for cases where the migration is applied outside an app deployment,
            // e.g. with the EF Core CLI command `dotnet ef database update command`, when no environment variable
            // is set. This also applies to running integration tests which identify as a Production environment
            // if the enviroment is not set. Integration tests connect as the Postgres superuser without running
            // the Docker script.
            migrationBuilder.Sql(
                 $"""
                  DO $$
                  BEGIN
                      IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '{PublicDataDbContext.PublicDataReadWriteRole}') THEN
                          GRANT SELECT, INSERT, UPDATE, DELETE, TRUNCATE, REFERENCES ON ALL TABLES IN SCHEMA public TO {PublicDataDbContext.PublicDataReadWriteRole};
                          GRANT SELECT, UPDATE ON ALL SEQUENCES IN SCHEMA public TO {PublicDataDbContext.PublicDataReadWriteRole};

                          ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE, TRUNCATE, REFERENCES ON TABLES TO {PublicDataDbContext.PublicDataReadWriteRole};
                          ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, UPDATE ON SEQUENCES TO {PublicDataDbContext.PublicDataReadWriteRole};
                      END IF;
                  END $$;
                  """);
        }
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
