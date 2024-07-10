using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Hosting;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class EES5235_GrantSequencePrivileges : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var isLocalEnvironment = 
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development;

        // Grant privileges on sequences created by this resource's database user to the Public API Data Processor Function App user.
        var dataProcessorFunctionAppRoleName = isLocalEnvironment
            ? "app_public_data_processor" 
            : Environment.GetEnvironmentVariable("DataProcessorFunctionAppIdentityName");
        if (dataProcessorFunctionAppRoleName != null)
        {
            migrationBuilder.Sql(
                $"""GRANT SELECT, UPDATE ON ALL SEQUENCES IN SCHEMA public TO {dataProcessorFunctionAppRoleName}""");
            migrationBuilder.Sql(
                $"""ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, UPDATE ON SEQUENCES TO {dataProcessorFunctionAppRoleName}""");
        }
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
