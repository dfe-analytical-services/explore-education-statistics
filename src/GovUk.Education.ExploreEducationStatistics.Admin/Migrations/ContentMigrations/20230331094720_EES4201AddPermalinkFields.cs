using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

[ExcludeFromCodeCoverage]
public partial class EES4201AddPermalinkFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
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
    }
}
