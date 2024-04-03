using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations;

/// <inheritdoc />
public partial class EES4807_AddPermissionsToDataProcessorUser : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // This migration grants permissions on database tables created by this resource's database user to the Data
        // Processor user.
        var dataProcessorFunctionAppIdentityName = Environment.GetEnvironmentVariable("DataProcessorFunctionAppIdentityName");

        if (dataProcessorFunctionAppIdentityName != null)
        {
            migrationBuilder.Sql(
                $"GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO \"{dataProcessorFunctionAppIdentityName}\"");
        }
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
