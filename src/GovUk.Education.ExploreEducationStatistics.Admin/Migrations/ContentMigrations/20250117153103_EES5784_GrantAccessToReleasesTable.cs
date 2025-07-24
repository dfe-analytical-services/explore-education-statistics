#nullable disable

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
// ReSharper disable once InconsistentNaming
public partial class EES5784_GrantAccessToReleasesTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("GRANT SELECT ON [dbo].[Releases] TO [data]");
        migrationBuilder.Sql("GRANT ALL ON [dbo].[Releases] TO [importer]");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
