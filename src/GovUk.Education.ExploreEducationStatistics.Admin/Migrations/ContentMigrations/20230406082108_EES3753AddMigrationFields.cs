using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

[ExcludeFromCodeCoverage]
public partial class EES3753AddMigrationFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add temporary columns to assist with the migration to Permalinks snapshots

        migrationBuilder.AddColumn<bool>(
            name: "Legacy",
            table: "Permalinks",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "LegacyHasSnapshot",
            table: "Permalinks",
            type: "bit",
            nullable: true);

        // Set the Legacy flag to true for all existing Permalinks so we can differentiate
        // between legacy Permalinks and Permalinks snapshots while both routes exist
        migrationBuilder.Sql("UPDATE Permalinks SET Legacy = 1");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Legacy",
            table: "Permalinks");

        migrationBuilder.DropColumn(
            name: "LegacyHasSnapshot",
            table: "Permalinks");
    }
}
