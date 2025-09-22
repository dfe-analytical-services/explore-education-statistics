using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations;

// ReSharper disable once InconsistentNaming
public partial class EES6080_AddActivatesInPreviewTokensTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "Activates",
            table: "PreviewTokens",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.Sql(
            @"UPDATE ""PreviewTokens"" SET ""Activates"" = ""Created"";");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "Activates",
            table: "PreviewTokens",
            nullable: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Activates",
            table: "PreviewTokens");
    }
}
