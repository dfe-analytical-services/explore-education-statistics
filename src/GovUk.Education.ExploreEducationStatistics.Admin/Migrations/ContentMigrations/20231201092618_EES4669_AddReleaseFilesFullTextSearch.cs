using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

[ExcludeFromCodeCoverage]
// ReSharper disable once InconsistentNaming
public partial class EES4669_AddReleaseFilesFullTextSearch : Migration
{
    private const string MigrationId = "20231201092618";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add a full-text catalog with an index on the ReleaseFiles Name and Summary columns
        migrationBuilder.SqlFromFile(ContentMigrationsPath,
            $"{MigrationId}_{nameof(EES4669_AddReleaseFilesFullTextSearch)}_FullTextIndex.sql",
            suppressTransaction: true);

        // Add a database table-valued function wrapping the SQL Server built-in function freetexttable
        // which can be mapped to a queryable function in EF Core.
        migrationBuilder.SqlFromFile(ContentMigrationsPath,
            $"{MigrationId}_{nameof(EES4669_AddReleaseFilesFullTextSearch)}_Routine_ReleaseFilesFreeTextTable.sql");

        migrationBuilder.Sql("GRANT SELECT ON dbo.ReleaseFilesFreeTextTable TO [content]");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("REVOKE SELECT ON dbo.ReleaseFilesFreeTextTable TO [content]");
        migrationBuilder.Sql("DROP FUNCTION dbo.ReleaseFilesFreeTextTable");
        migrationBuilder.Sql("DROP FULLTEXT INDEX ON dbo.ReleaseFiles", suppressTransaction: true);
        migrationBuilder.Sql("DROP FULLTEXT CATALOG ReleaseFilesFullTextCatalog", suppressTransaction: true);
    }
}
