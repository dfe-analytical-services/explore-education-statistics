using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class ReleaseSummaryModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReleaseName",
                table: "Releases",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Releases",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                columns: new[] { "ReleaseName", "Summary", "Title" },
                values: new object[] { "2016-17", "<p class=\"govuk-body\"> This service helps parents, specialists and the public find different kinds of pupil absence facts and figures for state-funded schools.</p><p class=\"govuk-body\">It allows you to find out about, view and download overall, authorised and unauthorised absence data and statistics going back to 2006/07 on the following levels:</p>", "Pupil absence data and statistics for schools in England" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReleaseName",
                table: "Releases");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Releases");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "Title",
                value: "2016-17");
        }
    }
}
