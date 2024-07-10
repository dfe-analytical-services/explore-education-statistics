using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class EES5329_CreatePublicDataAdminRole : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Grant privileges on objects created by this resource's database user to the 'public_data_admin' role.
        // The 'public_data_admin' role represents a group of admin users. Membership of it will be granted to
        // indvidual user roles who require write privileges on public schema objects.
        // Locally this is created in the initialisation script run by the Docker entrypoint.
        // In Azure this role needs to be created manually after the database is created.
        migrationBuilder.Sql(
            """
            DO $$
            BEGIN
                IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'public_data_admin') THEN
                    GRANT SELECT, INSERT, UPDATE, DELETE, TRUNCATE, REFERENCES ON ALL TABLES IN SCHEMA public TO public_data_admin;
                    GRANT SELECT, UPDATE ON ALL SEQUENCES IN SCHEMA public TO public_data_admin;
                    ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE, TRUNCATE, REFERENCES ON TABLES TO public_data_admin;
                    ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, UPDATE ON SEQUENCES TO public_data_admin;
                END IF;
            END $$;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}

