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
        var dataProcessorFunctionAppIdentityName = Environment.GetEnvironmentVariable("DataProcessorFunctionAppIdentityName");
        migrationBuilder.Sql($"GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO \"{dataProcessorFunctionAppIdentityName}\"");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
