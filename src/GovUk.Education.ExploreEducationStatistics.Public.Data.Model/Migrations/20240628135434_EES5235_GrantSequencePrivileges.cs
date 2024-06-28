using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class EES5235_GrantSequencePrivileges : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Grant privileges on sequences created by this resource's database user to the Public API Data Processor Function App user.
        var dataProcessorFunctionAppIdentityName = Environment.GetEnvironmentVariable("DataProcessorFunctionAppIdentityName");
        if (dataProcessorFunctionAppIdentityName != null)
        {
            migrationBuilder.Sql(
                $"GRANT SELECT, UPDATE ON ALL SEQUENCES IN SCHEMA public TO \"{dataProcessorFunctionAppIdentityName}\"");
            migrationBuilder.Sql(
                $"ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, UPDATE ON SEQUENCES TO \"{dataProcessorFunctionAppIdentityName}\"");
        }
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
