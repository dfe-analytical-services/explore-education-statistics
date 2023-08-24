using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

[ExcludeFromCodeCoverage]
public partial class EES3755_RemoveTemporaryFieldsFromPermalink : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Revoke the UPDATE privilege from the Permalinks table which was granted by EES-3754
        // to allow setting the LegacyHasSnapshot flag during the EES-4236 migration run.
        migrationBuilder.Sql("REVOKE UPDATE ON dbo.Permalinks FROM [data]");

        migrationBuilder.DropColumn(
            name: "CountFilterItems",
            table: "Permalinks");

        migrationBuilder.DropColumn(
            name: "CountFootnotes",
            table: "Permalinks");

        migrationBuilder.DropColumn(
            name: "CountIndicators",
            table: "Permalinks");

        migrationBuilder.DropColumn(
            name: "CountLocations",
            table: "Permalinks");

        migrationBuilder.DropColumn(
            name: "CountObservations",
            table: "Permalinks");

        migrationBuilder.DropColumn(
            name: "CountTimePeriods",
            table: "Permalinks");

        migrationBuilder.DropColumn(
            name: "LegacyContentLength",
            table: "Permalinks");

        migrationBuilder.DropColumn(
            name: "LegacyHasConfigurationHeaders",
            table: "Permalinks");

        migrationBuilder.DropColumn(
            name: "LegacyHasSnapshot",
            table: "Permalinks");

        // Legacy column retained to facilitate fallback to legacy data for resolving migration related issues.
        migrationBuilder.RenameColumn(
            name: "Legacy",
            table: "Permalinks",
            newName: "MigratedFromLegacy");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("GRANT UPDATE ON dbo.Permalinks TO [data]");

        migrationBuilder.RenameColumn(
            name: "MigratedFromLegacy",
            table: "Permalinks",
            newName: "Legacy");

        migrationBuilder.AddColumn<int>(
            name: "CountFilterItems",
            table: "Permalinks",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "CountFootnotes",
            table: "Permalinks",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "CountIndicators",
            table: "Permalinks",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "CountLocations",
            table: "Permalinks",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "CountObservations",
            table: "Permalinks",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "CountTimePeriods",
            table: "Permalinks",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<long>(
            name: "LegacyContentLength",
            table: "Permalinks",
            type: "bigint",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "LegacyHasConfigurationHeaders",
            table: "Permalinks",
            type: "bit",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "LegacyHasSnapshot",
            table: "Permalinks",
            type: "bit",
            nullable: true);
    }
}
