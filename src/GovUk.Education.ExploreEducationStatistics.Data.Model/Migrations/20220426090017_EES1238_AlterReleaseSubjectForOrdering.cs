using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1238_AlterReleaseSubjectForOrdering : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "ReleaseSubject",
                type: "datetime2",
                nullable: true);

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

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "ReleaseSubject",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "ReleaseSubject");

            migrationBuilder.DropColumn(
                name: "FilterSequence",
                table: "ReleaseSubject");

            migrationBuilder.DropColumn(
                name: "IndicatorSequence",
                table: "ReleaseSubject");

            migrationBuilder.DropColumn(
                name: "Updated",
                table: "ReleaseSubject");
        }
    }
}
