using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class HtmlToMarkdown : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "Summary",
                value: @"This service helps parents, specialists and the public find different kinds of pupil absence facts and figures for state-funded schools.

It allows you to find out about, view and download overall, authorised and unauthorised absence data and statistics going back to 2006/07 on the following levels:");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "Summary",
                value: "<p class=\"govuk-body\"> This service helps parents, specialists and the public find different kinds of pupil absence facts and figures for state-funded schools.</p><p class=\"govuk-body\">It allows you to find out about, view and download overall, authorised and unauthorised absence data and statistics going back to 2006/07 on the following levels:</p>");
        }
    }
}
