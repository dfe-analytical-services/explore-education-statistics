using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations;

/// <inheritdoc />
public partial class EES5563_AddDeletedMetaFlagsToDataSetVersionMapping : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "HasDeletedGeographicLevels",
            table: "DataSetVersionMappings",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "HasDeletedIndicators",
            table: "DataSetVersionMappings",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "HasDeletedTimePeriods",
            table: "DataSetVersionMappings",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "HasDeletedGeographicLevels",
            table: "DataSetVersionMappings");

        migrationBuilder.DropColumn(
            name: "HasDeletedIndicators",
            table: "DataSetVersionMappings");

        migrationBuilder.DropColumn(
            name: "HasDeletedTimePeriods",
            table: "DataSetVersionMappings");
    }
}
