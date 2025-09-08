#nullable disable

using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
// ReSharper disable once InconsistentNaming
public partial class EES6317_AddReleaseVersionPublishingOrganisations : Migration
{
    private const string MigrationId = "20250714103833";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Organisations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Url = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                Updated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Organisations", x => x.Id);
            }
        );

        migrationBuilder.CreateTable(
            name: "ReleaseVersionPublishingOrganisations",
            columns: table => new
            {
                OrganisationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReleaseVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_ReleaseVersionPublishingOrganisations",
                    x => new { x.OrganisationId, x.ReleaseVersionId }
                );
                table.ForeignKey(
                    name: "FK_ReleaseVersionPublishingOrganisations_Organisations_OrganisationId",
                    column: x => x.OrganisationId,
                    principalTable: "Organisations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_ReleaseVersionPublishingOrganisations_ReleaseVersions_ReleaseVersionId",
                    column: x => x.ReleaseVersionId,
                    principalTable: "ReleaseVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "IX_Organisations_Title",
            table: "Organisations",
            column: "Title",
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_ReleaseVersionPublishingOrganisations_ReleaseVersionId",
            table: "ReleaseVersionPublishingOrganisations",
            column: "ReleaseVersionId"
        );

        migrationBuilder.Sql("GRANT SELECT ON dbo.Organisations TO [content];");
        migrationBuilder.Sql("GRANT SELECT ON dbo.ReleaseVersionPublishingOrganisations TO [content];");
        migrationBuilder.Sql("GRANT SELECT ON dbo.Organisations TO [publisher];");
        migrationBuilder.Sql("GRANT SELECT ON dbo.ReleaseVersionPublishingOrganisations TO [publisher];");

        // Insert seed Organisations
        migrationBuilder.SqlFromFile(
            MigrationConstants.ContentMigrationsPath,
            $"{MigrationId}_{nameof(EES6317_AddReleaseVersionPublishingOrganisations)}.sql"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("REVOKE SELECT ON dbo.Organisations TO [content];");
        migrationBuilder.Sql("REVOKE SELECT ON dbo.ReleaseVersionPublishingOrganisations TO [content];");
        migrationBuilder.Sql("REVOKE SELECT ON dbo.Organisations TO [publisher];");
        migrationBuilder.Sql("REVOKE SELECT ON dbo.ReleaseVersionPublishingOrganisations TO [publisher];");

        migrationBuilder.DropTable(name: "ReleaseVersionPublishingOrganisations");

        migrationBuilder.DropTable(name: "Organisations");
    }
}
