using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

public partial class EES4858_UpdateGeographicLevelCodesInLocations : Migration
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
                table: "Location",
                keyColumn: "GeographicLevel",
                keyValue: update.OldCode,
                column: "GeographicLevel",
                value: update.NewCode);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var update in Updates)
        {
            migrationBuilder.UpdateData(
                table: "Location",
                keyColumn: "GeographicLevel",
                keyValue: update.NewCode,
                column: "GeographicLevel",
                value: update.OldCode);
        }
    }
}
