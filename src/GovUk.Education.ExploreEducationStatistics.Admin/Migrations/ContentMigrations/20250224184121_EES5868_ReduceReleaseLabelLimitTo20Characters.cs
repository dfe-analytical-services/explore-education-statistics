#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
// ReSharper disable once InconsistentNaming
public partial class EES5868_ReduceReleaseLabelLimitTo20Characters : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Slug",
            table: "Releases",
            type: "nvarchar(51)",
            maxLength: 51,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(81)",
            oldMaxLength: 81
        );

        migrationBuilder.AlterColumn<string>(
            name: "Label",
            table: "Releases",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50,
            oldNullable: true
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Slug",
            table: "Releases",
            type: "nvarchar(81)",
            maxLength: 81,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(51)",
            oldMaxLength: 51
        );

        migrationBuilder.AlterColumn<string>(
            name: "Label",
            table: "Releases",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20,
            oldNullable: true
        );
    }
}
