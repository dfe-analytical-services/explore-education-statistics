using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

public partial class EES4886_UpdateGeographicLevelCodesInBoundaryLevels : Migration
{
    private readonly List<(string OldCode, string NewCode)> Updates = new()
    {
        (OldCode: "INS", NewCode: "INST"),
        (OldCode: "PC", NewCode: "PCON"),
        (OldCode: "PRO", NewCode: "PROV"),
        (OldCode: "RSCR", NewCode: "RSC"),
        (OldCode: "SPO", NewCode: "SPON"),
        (OldCode: "WAR", NewCode: "WARD"),
    };

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var update in Updates)
        {
            migrationBuilder.UpdateData(
                table: "BoundaryLevel",
                keyColumn: "Level",
                keyValue: update.OldCode,
                column: "Level",
                value: update.NewCode
            );
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var update in Updates)
        {
            migrationBuilder.UpdateData(
                table: "BoundaryLevel",
                keyColumn: "Level",
                keyValue: update.NewCode,
                column: "Level",
                value: update.OldCode
            );
        }
    }
}
