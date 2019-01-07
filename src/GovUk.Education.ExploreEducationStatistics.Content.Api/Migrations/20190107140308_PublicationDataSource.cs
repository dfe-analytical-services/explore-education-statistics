using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class PublicationDataSource : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataSource",
                table: "Publications",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                column: "DataSource",
                value: "[Pupil absence statistics: guide](https://www.gov.uk/government/publications/absence-statistics-guide#)");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "Summary",
                value: @"This service helps parents, specialists and the public find different kinds of pupil absence facts and figures for state-funded schools.

It allows you to find out about, view and download overall, authorised and unauthorised absence data and statistics going back to 2006/07 on the following levels:");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                column: "Summary",
                value: @"This service helps parents, specialists and the public find different kinds of pupil absence facts and figures for state-funded schools.

It allows you to find out about, view and download overall, authorised and unauthorised absence data and statistics going back to 2006/07 on the following levels:");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataSource",
                table: "Publications");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "Summary",
                value: @"This service helps parents, specialists and the public find different kinds of pupil absence facts and figures for state-funded schools.

It allows you to find out about, view and download overall, authorised and unauthorised absence data and statistics going back to 2006/07 on the following levels:");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                column: "Summary",
                value: @"This service helps parents, specialists and the public find different kinds of pupil absence facts and figures for state-funded schools.

It allows you to find out about, view and download overall, authorised and unauthorised absence data and statistics going back to 2006/07 on the following levels:");
        }
    }
}
