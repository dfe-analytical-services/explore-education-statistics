using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class Ees6063DropFunctionPublicationsFreeTextTable : Migration
{
    private const string PreviousPublicationsFreeTextTableMigrationId =
        EES3661_AddPublicationsFreeTextTableFunction.MigrationId;

    private const string PreviousPublicationsFullTextCatalogAndIndexMigrationId =
        EES3661_AddPublicationsFullTextCatalogAndIndex.MigrationId;

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("REVOKE SELECT ON dbo.PublicationsFreeTextTable TO [content]");
        migrationBuilder.Sql("DROP FUNCTION dbo.PublicationsFreeTextTable");
        migrationBuilder.Sql("DROP FULLTEXT INDEX ON dbo.Publications", suppressTransaction: true);
        migrationBuilder.Sql("DROP FULLTEXT CATALOG PublicationsFullTextCatalog", suppressTransaction: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.SqlFromFile(
            ContentMigrationsPath,
            $"{PreviousPublicationsFullTextCatalogAndIndexMigrationId}_PublicationsFullTextCatalogAndIndex.sql",
            suppressTransaction: true
        );
        migrationBuilder.SqlFromFile(
            ContentMigrationsPath,
            $"{PreviousPublicationsFreeTextTableMigrationId}_Routine_PublicationsFreeTextTable.sql"
        );
        migrationBuilder.Sql("GRANT SELECT ON dbo.PublicationsFreeTextTable TO [content]");
    }
}
