using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class AddReleaseSummaryVersioningCopy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ReleaseSummaries",
                columns: new[] { "Id", "ReleaseId" },
                values: new object[,]
                {
                    { new Guid("1bf7c51f-4d12-4697-8868-455760a887a7"), new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") },
                    { new Guid("51eb730b-d76c-4a0c-aaf2-cf7aa96f133a"), new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d") },
                    { new Guid("06c45b1e-533d-4c95-900b-62beb4620f59"), new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") },
                    { new Guid("c6e08ed3-d93a-410a-9e7e-600f2cf25725"), new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717") }
                });

            migrationBuilder.InsertData(
                table: "ReleaseSummaryVersions",
                columns: new[] { "Id", "Created", "NextReleaseDate", "PublishScheduled", "ReleaseName", "ReleaseSummaryId", "Slug", "Summary", "TimePeriodCoverage", "TypeId" },
                values: new object[,]
                {
                    { new Guid("420ca58e-278b-456b-9031-fe74a6966159"), new DateTime(2018, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "2016", new Guid("1bf7c51f-4d12-4697-8868-455760a887a7"), "2016-17", @"Read national statistical summaries, view charts and tables and download data files.

                Find out how and why these statistics are collected and published - [Pupil absence statistics: methodology](../methodology/pupil-absence-in-schools-in-england).", "AY", new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c") },
                    { new Guid("fe5e8cac-a574-4e83-861b-7b5f927d7d34"), new DateTime(2016, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "2015", new Guid("51eb730b-d76c-4a0c-aaf2-cf7aa96f133a"), "2015-16", "Read national statistical summaries and definitions, view charts and tables and download data files across a range of pupil absence subject areas.", "AY", new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c") },
                    { new Guid("04adfe47-9057-4abd-a0e8-5a6ac56e1560"), new DateTime(2018, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "2016", new Guid("06c45b1e-533d-4c95-900b-62beb4620f59"), "2016-17", @"Read national statistical summaries, view charts and tables and download data files.

                Find out how and why these statistics are collected and published - [Permanent and fixed-period exclusion statistics: methodology](../methodology/permanent-and-fixed-period-exclusions-in-england)", "AY", new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c") },
                    { new Guid("c6e08ed3-d93a-410a-9e7e-600f2cf25725"), new DateTime(2018, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "2018", new Guid("c6e08ed3-d93a-410a-9e7e-600f2cf25725"), "2018", @"Read national statistical summaries, view charts and tables and download data files.

                Find out how and why these statistics are collected and published - [Secondary and primary school applications and offers: methodology](../methodology/secondary-and-primary-schools-applications-and-offers)", "AY", new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c") }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ReleaseSummaryVersions",
                keyColumn: "Id",
                keyValue: new Guid("04adfe47-9057-4abd-a0e8-5a6ac56e1560"));

            migrationBuilder.DeleteData(
                table: "ReleaseSummaryVersions",
                keyColumn: "Id",
                keyValue: new Guid("420ca58e-278b-456b-9031-fe74a6966159"));

            migrationBuilder.DeleteData(
                table: "ReleaseSummaryVersions",
                keyColumn: "Id",
                keyValue: new Guid("c6e08ed3-d93a-410a-9e7e-600f2cf25725"));

            migrationBuilder.DeleteData(
                table: "ReleaseSummaryVersions",
                keyColumn: "Id",
                keyValue: new Guid("fe5e8cac-a574-4e83-861b-7b5f927d7d34"));

            migrationBuilder.DeleteData(
                table: "ReleaseSummaries",
                keyColumn: "Id",
                keyValue: new Guid("06c45b1e-533d-4c95-900b-62beb4620f59"));

            migrationBuilder.DeleteData(
                table: "ReleaseSummaries",
                keyColumn: "Id",
                keyValue: new Guid("1bf7c51f-4d12-4697-8868-455760a887a7"));

            migrationBuilder.DeleteData(
                table: "ReleaseSummaries",
                keyColumn: "Id",
                keyValue: new Guid("51eb730b-d76c-4a0c-aaf2-cf7aa96f133a"));

            migrationBuilder.DeleteData(
                table: "ReleaseSummaries",
                keyColumn: "Id",
                keyValue: new Guid("c6e08ed3-d93a-410a-9e7e-600f2cf25725"));
        }
    }
}
