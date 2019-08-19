using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class AddNameFieldToFilterAndIndicator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Indicator",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Filter",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Indicator_Name",
                table: "Indicator",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Filter_Name",
                table: "Filter",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Indicator_Name",
                table: "Indicator");

            migrationBuilder.DropIndex(
                name: "IX_Filter_Name",
                table: "Filter");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Indicator");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Filter");
        }
    }
}
