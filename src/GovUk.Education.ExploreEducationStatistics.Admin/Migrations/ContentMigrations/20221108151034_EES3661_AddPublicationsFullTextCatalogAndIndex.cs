using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

[ExcludeFromCodeCoverage]
public partial class EES3661_AddPublicationsFullTextCatalogAndIndex : Migration
{
    private const string MigrationId = "20221108151034";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add a full-text catalog with an index on the Publications Summary and Title columns
        migrationBuilder.SqlFromFile(ContentMigrationsPath, $"{MigrationId}_PublicationsFullTextCatalogAndIndex.sql",
            suppressTransaction: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP FULLTEXT INDEX ON dbo.Publications", suppressTransaction: true);
        migrationBuilder.Sql("DROP FULLTEXT CATALOG PublicationsFullTextCatalog", suppressTransaction: true);
    }
}
