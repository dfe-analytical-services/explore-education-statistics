using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class ExclusionPublication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("e3a532c4-df72-4daf-b621-5d04418fd521"), "2015 to 2016", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2015-to-2016" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("a0a8999a-9580-48b8-b443-61446ea579e4"), "2014 to 2015", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2014-to-2015" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("97c8ee35-4c62-406c-880a-cdfc92590490"), "2013 to 2014", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2013-to-2014" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("d76afaed-a665-4366-8897-78e9b90aa28a"), "2012 to 2013", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2012-to-2013" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("7f6c5499-640a-44c7-afdc-41e78c7e8b24"), "2011 to 2012", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-2011-to-2012-academic-year" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("8c12b38a-a071-4c47-ba16-dffa734849ed"), "2010 to 2011", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2010-to-2011" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("04ebb3e6-67fd-41f3-89e4-ed9566bcbe96"), "2009 to 2010", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2009-to-2010" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("1b9375dd-06c7-4391-8265-447d6992a853"), "2008 to 2009", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-academic-year-2008-to-2009" });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "Summary",
                value: "This service helps parents, specialists and the public find different kinds of pupil absence facts and figures for state-funded schools. It allows you to find out about, view and download overall, authorised and unauthorised absence data and statistics going back to 2006/07 on the following levels:");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                column: "Summary",
                value: "This service helps parents, specialists and the public find different kinds of pupil absence facts and figures for state-funded schools. It allows you to find out about, view and download overall, authorised and unauthorised absence data and statistics going back to 2006/07 on the following levels:");

            migrationBuilder.InsertData(
                table: "Releases",
                columns: new[] { "Id", "PublicationId", "Published", "ReleaseName", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), new DateTime(2018, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "2016 to 2017", "2016-17", "Read national statistical summaries and definitions, view charts and tables and download data files across a range of permanent and fixed-period exclusion subject areas.", "Permanent and fixed period exclusions" });

            migrationBuilder.UpdateData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("9c0f0139-7f88-4750-afe0-1c85cdf1d047"),
                column: "Reason",
                value: "Underlying data file updated to include absence data by pupil residency and school location, and updated metadata document.");

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("4fca874d-98b8-4c79-ad20-d698fb0af7dc"), new DateTime(2018, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "First published.", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("33ff3f17-0671-41e9-b404-5661ab8a9476"), new DateTime(2018, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), " Updated exclusion rates for Gypsy/Roma pupils, to include extended ethnicity categories within the headcount (Gypsy, Roma and other Gypsy/Roma). ", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("04ebb3e6-67fd-41f3-89e4-ed9566bcbe96"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("1b9375dd-06c7-4391-8265-447d6992a853"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("7f6c5499-640a-44c7-afdc-41e78c7e8b24"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("8c12b38a-a071-4c47-ba16-dffa734849ed"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("97c8ee35-4c62-406c-880a-cdfc92590490"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("a0a8999a-9580-48b8-b443-61446ea579e4"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("d76afaed-a665-4366-8897-78e9b90aa28a"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("e3a532c4-df72-4daf-b621-5d04418fd521"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("33ff3f17-0671-41e9-b404-5661ab8a9476"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("4fca874d-98b8-4c79-ad20-d698fb0af7dc"));

            migrationBuilder.DeleteData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"));

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "Summary",
                value: "This service helps parents, specialists and the public find different kinds of pupil absence facts and figures for state-funded schools. It allows you to find out about, view and download overall, authorised and unauthorised absence data and statistics going back to 2006/07 on the following levels:");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                column: "Summary",
                value: "This service helps parents, specialists and the public find different kinds of pupil absence facts and figures for state-funded schools. It allows you to find out about, view and download overall, authorised and unauthorised absence data and statistics going back to 2006/07 on the following levels:");

            migrationBuilder.UpdateData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("9c0f0139-7f88-4750-afe0-1c85cdf1d047"),
                column: "Reason",
                value: "Underlying data file updated to include absence data by pupil residency and school location, andupdated metadata document.");
        }
    }
}
