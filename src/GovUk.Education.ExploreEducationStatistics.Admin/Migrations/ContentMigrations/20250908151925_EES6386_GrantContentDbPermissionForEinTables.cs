using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
public partial class EES6386_GrantContentDbPermissionForEinTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("GRANT SELECT ON dbo.EducationInNumbersPages TO [content]");
        migrationBuilder.Sql("GRANT SELECT ON dbo.EinContentSections TO [content]");
        migrationBuilder.Sql("GRANT SELECT ON dbo.EinContentBlocks TO [content]");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("REVOKE SELECT ON dbo.EducationInNumbersPages TO [content]");
        migrationBuilder.Sql("REVOKE SELECT ON dbo.EinContentSections TO [content]");
        migrationBuilder.Sql("REVOKE SELECT ON dbo.EinContentBlocks TO [content]");
    }
}
