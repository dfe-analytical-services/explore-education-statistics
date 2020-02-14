using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class AddTimeIdentifierAndYear : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Release");

            migrationBuilder.AddColumn<int>(
                name: "TimeIdentifier",
                table: "Release",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Release",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeIdentifier",
                table: "Release");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Release");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Release",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
