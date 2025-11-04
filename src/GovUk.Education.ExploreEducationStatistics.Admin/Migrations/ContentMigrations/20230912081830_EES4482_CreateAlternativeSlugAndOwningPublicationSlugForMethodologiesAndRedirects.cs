#nullable disable

using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

// ReSharper disable once InconsistentNaming
public partial class EES4482_CreateAlternativeSlugAndOwningPublicationSlugForMethodologiesAndRedirects : Migration
{
    private const string MigrationId = "20230912081830";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "AlternativeSlug",
            table: "MethodologyVersions",
            type: "nvarchar(max)",
            nullable: true
        );

        migrationBuilder.AddColumn<string>(
            name: "OwningPublicationSlug",
            table: "Methodologies",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: ""
        );

        migrationBuilder.CreateTable(
            name: "MethodologyRedirects",
            columns: table => new
            {
                Slug = table.Column<string>(type: "nvarchar(450)", nullable: false),
                MethodologyVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MethodologyRedirects", x => new { x.MethodologyVersionId, x.Slug });
                table.ForeignKey(
                    name: "FK_MethodologyRedirects_MethodologyVersions_MethodologyVersionId",
                    column: x => x.MethodologyVersionId,
                    principalTable: "MethodologyVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.Sql("GRANT SELECT ON dbo.MethodologyRedirects TO [content]");
        migrationBuilder.Sql("GRANT SELECT ON dbo.MethodologyRedirects TO [publisher]");

        migrationBuilder.SqlFromFile(
            MigrationConstants.ContentMigrationsPath,
            $"{MigrationId}_EES4482_MigrateMethodologyOwningPublicationSlug.sql"
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "MethodologyRedirects");

        migrationBuilder.DropColumn(name: "AlternativeSlug", table: "MethodologyVersions");

        migrationBuilder.DropColumn(name: "OwningPublicationSlug", table: "Methodologies");
    }
}
