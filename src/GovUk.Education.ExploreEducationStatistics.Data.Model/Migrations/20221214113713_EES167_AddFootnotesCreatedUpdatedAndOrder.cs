using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

[ExcludeFromCodeCoverage]
public partial class EES167_AddFootnotesCreatedUpdatedAndOrder : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Order",
            table: "Footnote",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<DateTime>(
            name: "Created",
            table: "Footnote",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "Updated",
            table: "Footnote",
            type: "datetime2",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Order",
            table: "Footnote");

        migrationBuilder.DropColumn(
            name: "Created",
            table: "Footnote");

        migrationBuilder.DropColumn(
            name: "Updated",
            table: "Footnote");
    }
}
