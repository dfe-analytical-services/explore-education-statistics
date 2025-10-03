using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

/// <inheritdoc />
public partial class EES4735_AddAutoSelectFilterItemColToFiltersTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "AutoSelectFilterItemId",
            table: "Filter",
            type: "uniqueidentifier",
            nullable: true
        );

        migrationBuilder.AddColumn<string>(
            name: "AutoSelectFilterItemLabel",
            table: "Filter",
            type: "nvarchar(max)",
            nullable: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_Filter_AutoSelectFilterItemId",
            table: "Filter",
            column: "AutoSelectFilterItemId"
        );

        migrationBuilder.AddForeignKey(
            name: "FK_Filter_FilterItem_AutoSelectFilterItemId",
            table: "Filter",
            column: "AutoSelectFilterItemId",
            principalTable: "FilterItem",
            principalColumn: "Id"
        );

        // Fix casing all all preexisting totals to "Total" - WHERE is case agonostic (i.e. matches any casing of "total")
        migrationBuilder.Sql(
            """
            UPDATE FilterItem
            SET Label = 'Total'
            WHERE Label = 'total';
            """
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_Filter_FilterItem_AutoSelectFilterItemId", table: "Filter");

        migrationBuilder.DropIndex(name: "IX_Filter_AutoSelectFilterItemId", table: "Filter");

        migrationBuilder.DropColumn(name: "AutoSelectFilterItemId", table: "Filter");

        migrationBuilder.DropColumn(name: "AutoSelectFilterItemLabel", table: "Filter");
    }
}
