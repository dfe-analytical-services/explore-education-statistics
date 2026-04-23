#nullable disable

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

[ExcludeFromCodeCoverage]
// ReSharper disable once InconsistentNaming
public partial class EES3964_DropPublicationLegacyPublicationUrl : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "LegacyPublicationUrl", table: "Publications");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "LegacyPublicationUrl",
            table: "Publications",
            type: "nvarchar(max)",
            nullable: true
        );
    }
}
