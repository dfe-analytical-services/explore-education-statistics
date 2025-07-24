using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
// ReSharper disable once InconsistentNaming
public partial class EES6240_UpdateReleasePublishingFeedbackTableColumnsAndPermissions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Response",
            table: "ReleasePublishingFeedback",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50);

        migrationBuilder.Sql("REVOKE INSERT ON dbo.ReleasePublishingFeedback TO [content];");
        migrationBuilder.Sql("GRANT SELECT, UPDATE ON dbo.ReleasePublishingFeedback TO [content];");
        
        migrationBuilder.Sql("GRANT SELECT, INSERT ON dbo.ReleasePublishingFeedback TO [publisher];");
        migrationBuilder.Sql("GRANT SELECT ON dbo.UserPublicationRoles TO [publisher];");
        migrationBuilder.Sql("GRANT SELECT ON dbo.Users TO [publisher];");
        
        migrationBuilder.Sql("GRANT SELECT ON dbo.ReleasePublishingFeedback TO [notifier];");
        migrationBuilder.Sql("GRANT SELECT ON dbo.Releases TO [notifier];");
        migrationBuilder.Sql("GRANT SELECT ON dbo.ReleaseVersions TO [notifier];");
        migrationBuilder.Sql("GRANT SELECT ON dbo.Publications TO [notifier];");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Response",
            table: "ReleasePublishingFeedback",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50,
            oldNullable: true);
    }
}
