using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

[ExcludeFromCodeCoverage]
public partial class EES4574_AddLsipGeographicLevel : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "LocalSkillsImprovementPlanArea_Code",
            table: "Location",
            type: "nvarchar(450)",
            nullable: true
        );

        migrationBuilder.AddColumn<string>(
            name: "LocalSkillsImprovementPlanArea_Name",
            table: "Location",
            type: "nvarchar(max)",
            nullable: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_Location_LocalSkillsImprovementPlanArea_Code",
            table: "Location",
            column: "LocalSkillsImprovementPlanArea_Code"
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Location_LocalSkillsImprovementPlanArea_Code", table: "Location");

        migrationBuilder.DropColumn(name: "LocalSkillsImprovementPlanArea_Code", table: "Location");

        migrationBuilder.DropColumn(name: "LocalSkillsImprovementPlanArea_Name", table: "Location");
    }
}
