#nullable disable

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
// ReSharper disable once InconsistentNaming
public partial class EES4975_AddKeyStatisticsCascadeDeletes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Abstract type KeyStatistics uses the TPT strategy.
        // There's a cascade delete which means that when a KeyStatistics base record is
        // deleted, corresponding KeyStatisticsDataBlock and KeyStatisticsText sub-type records are also deleted.
        // In EF Core 6.0 a bug in the SQL Server database provider meant that these cascade deletes were not created,
        // and this cascade delete was handled client-side.
        // This was fixed by EF Core 7.0.
        // See https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/breaking-changes?tabs=v7#tpt-cascade-delete

        // Remove the non-cascading foreign key constraints
        migrationBuilder.DropForeignKey(
            name: "FK_KeyStatisticsDataBlock_KeyStatistics_Id",
            table: "KeyStatisticsDataBlock");

        migrationBuilder.DropForeignKey(
            name: "FK_KeyStatisticsText_KeyStatistics_Id",
            table: "KeyStatisticsText");

        // Add the cascading foreign key constraints
        migrationBuilder.AddForeignKey(
            name: "FK_KeyStatisticsDataBlock_KeyStatistics_Id",
            table: "KeyStatisticsDataBlock",
            column: "Id",
            principalTable: "KeyStatistics",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_KeyStatisticsText_KeyStatistics_Id",
            table: "KeyStatisticsText",
            column: "Id",
            principalTable: "KeyStatistics",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_KeyStatisticsDataBlock_KeyStatistics_Id",
            table: "KeyStatisticsDataBlock");

        migrationBuilder.DropForeignKey(
            name: "FK_KeyStatisticsText_KeyStatistics_Id",
            table: "KeyStatisticsText");

        migrationBuilder.AddForeignKey(
            name: "FK_KeyStatisticsDataBlock_KeyStatistics_Id",
            table: "KeyStatisticsDataBlock",
            column: "Id",
            principalTable: "KeyStatistics",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_KeyStatisticsText_KeyStatistics_Id",
            table: "KeyStatisticsText",
            column: "Id",
            principalTable: "KeyStatistics",
            principalColumn: "Id");
    }
}
