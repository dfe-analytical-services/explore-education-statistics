using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

[ExcludeFromCodeCoverage]
public partial class EES4661_DropReleaseSubjectDataGuidance : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "DataGuidance", table: "ReleaseSubject");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "DataGuidance",
            table: "ReleaseSubject",
            type: "nvarchar(max)",
            nullable: true
        );
    }
}
