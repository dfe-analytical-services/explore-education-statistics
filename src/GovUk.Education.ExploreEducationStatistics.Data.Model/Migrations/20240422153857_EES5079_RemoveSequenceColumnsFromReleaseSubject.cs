using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

/// <inheritdoc />
public partial class EES5079_RemoveSequenceColumnsFromReleaseSubject : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FilterSequence",
            table: "ReleaseSubject");

        migrationBuilder.DropColumn(
            name: "IndicatorSequence",
            table: "ReleaseSubject");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "FilterSequence",
            table: "ReleaseSubject",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "IndicatorSequence",
            table: "ReleaseSubject",
            type: "nvarchar(max)",
            nullable: true);
    }
}
