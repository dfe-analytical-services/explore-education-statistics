using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

[ExcludeFromCodeCoverage]
public partial class EES3754GrantUpdateOnPermalinks : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Grant UPDATE on Permalinks to allow setting the LegacyHasSnapshot flag during the EES-3754 migration
        migrationBuilder.Sql("GRANT UPDATE ON dbo.Permalinks TO [data]");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("REVOKE UPDATE ON dbo.Permalinks TO [data]");
    }
}
