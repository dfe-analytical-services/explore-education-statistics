using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class NewRelease : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Releases",
                columns: new[] { "Id", "PublicationId", "Published", "ReleaseName", "Summary", "Title" },
                values: new object[] { new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"), new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), new DateTime(2018, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "2015-16", @"This service helps parents, specialists and the public find different kinds of pupil absence facts and figures for state-funded schools.

It allows you to find out about, view and download overall, authorised and unauthorised absence data and statistics going back to 2006/07 on the following levels:", "Pupil absence data and statistics for schools in England" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"));
        }
    }
}
