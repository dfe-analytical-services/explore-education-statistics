#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
// ReSharper disable once InconsistentNaming
public partial class EES5627_AddingLabelToRelease : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Releases_PublicationId_Year_TimePeriodCoverage", table: "Releases");

        migrationBuilder.AlterColumn<string>(
            name: "Slug",
            table: "Releases",
            type: "nvarchar(81)",
            maxLength: 81,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(30)",
            oldMaxLength: 30
        );

        migrationBuilder.AddColumn<string>(
            name: "Label",
            table: "Releases",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_Releases_PublicationId_Year_TimePeriodCoverage_Label",
            table: "Releases",
            columns: new[] { "PublicationId", "Year", "TimePeriodCoverage", "Label" },
            unique: true
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Releases_PublicationId_Year_TimePeriodCoverage_Label", table: "Releases");

        migrationBuilder.DropColumn(name: "Label", table: "Releases");

        migrationBuilder.AlterColumn<string>(
            name: "Slug",
            table: "Releases",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(81)",
            oldMaxLength: 81
        );

        migrationBuilder.CreateIndex(
            name: "IX_Releases_PublicationId_Year_TimePeriodCoverage",
            table: "Releases",
            columns: new[] { "PublicationId", "Year", "TimePeriodCoverage" },
            unique: true
        );
    }
}
