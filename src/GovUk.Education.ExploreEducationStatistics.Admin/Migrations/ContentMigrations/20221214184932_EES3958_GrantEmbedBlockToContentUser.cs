using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

[ExcludeFromCodeCoverage]
public partial class EES3958_GrantEmbedBlockToContentUser : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add missing grant related to EES-3682
        migrationBuilder.Sql("GRANT SELECT ON dbo.EmbedBlocks TO [content]");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("REVOKE SELECT ON dbo.EmbedBlocks TO [content]");
    }
}
