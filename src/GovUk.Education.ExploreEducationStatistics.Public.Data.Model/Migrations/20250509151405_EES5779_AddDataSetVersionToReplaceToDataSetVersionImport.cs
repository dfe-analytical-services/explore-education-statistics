using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations;

/// <inheritdoc />
public partial class EES5779_AddDataSetVersionToReplaceToDataSetVersionImport : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "DataSetVersionToReplaceId",
            table: "DataSetVersionImports",
            type: "uuid",
            nullable: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_DataSetVersionImports_DataSetVersionToReplaceId",
            table: "DataSetVersionImports",
            column: "DataSetVersionToReplaceId"
        );

        migrationBuilder.AddForeignKey(
            name: "FK_DataSetVersionImports_DataSetVersions_DataSetVersionToRepla~",
            table: "DataSetVersionImports",
            column: "DataSetVersionToReplaceId",
            principalTable: "DataSetVersions",
            principalColumn: "Id"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_DataSetVersionImports_DataSetVersions_DataSetVersionToRepla~",
            table: "DataSetVersionImports"
        );

        migrationBuilder.DropIndex(
            name: "IX_DataSetVersionImports_DataSetVersionToReplaceId",
            table: "DataSetVersionImports"
        );

        migrationBuilder.DropColumn(name: "DataSetVersionToReplaceId", table: "DataSetVersionImports");
    }
}
