#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
// ReSharper disable once InconsistentNaming
public partial class EES5631_GrantPublisherAccessToReleasesAndReleaseRedirectsTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("GRANT SELECT ON dbo.Releases TO [content]");
        migrationBuilder.Sql("GRANT SELECT ON dbo.ReleaseRedirects TO [content]");

        migrationBuilder.Sql("GRANT SELECT ON dbo.Releases TO [publisher]");
        migrationBuilder.Sql("GRANT SELECT ON dbo.ReleaseRedirects TO [publisher]");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) { }
}
