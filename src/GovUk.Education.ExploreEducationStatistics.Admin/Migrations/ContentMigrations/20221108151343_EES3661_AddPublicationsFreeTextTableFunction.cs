using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

[ExcludeFromCodeCoverage]
public partial class EES3661_AddPublicationsFreeTextTableFunction : Migration
{
    private const string MigrationId = "20221108151343";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add a database table-valued function wrapping the SQL Server built-in function freetexttable.
        // This can be mapped to a queryable function in EF Core
        migrationBuilder.SqlFromFile(ContentMigrationsPath, $"{MigrationId}_Routine_PublicationsFreeTextTable.sql");
        migrationBuilder.Sql("GRANT SELECT ON dbo.PublicationsFreeTextTable TO [content]");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("REVOKE SELECT ON dbo.PublicationsFreeTextTable TO [content]");
        migrationBuilder.Sql("DROP FUNCTION dbo.PublicationsFreeTextTable");
    }
}
