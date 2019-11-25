using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class AddSummaryAndHeadlineContentSectionsToRelease : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Summary",
                table: "ReleaseSummaryVersions");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Releases");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "ContentSections",
                nullable: false,
                defaultValue: ContentSectionType.Generic);
            
            migrationBuilder.InsertData(
                table: "ContentSections",
                columns: new[] { "Id", "Caption", "Heading", "Order", "ReleaseId", "Type" },
                values: new object[,]
                {
                    { new Guid("8abdae8f-4119-41ac-8efd-2229b7ea31da"), "", "", 1, new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), "Headlines" },
                    { new Guid("601aadcc-be7d-4d3e-9154-c9eb64144692"), "", "", 1, new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), "Headlines" },
                    { new Guid("c0241ab7-f40a-4755-bc69-365eba8114a3"), "", "", 1, new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), "Headlines" },
                    { new Guid("93ef0486-479f-4013-8012-a66ed01f1880"), "", "", 1, new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), "ReleaseSummary" },
                    { new Guid("f599c2e2-f215-423a-beab-c5c6a0c2e5a9"), "", "", 1, new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), "ReleaseSummary" },
                    { new Guid("4f30b382-ce28-4a3e-801a-ce76004f5eb4"), "", "", 1, new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), "ReleaseSummary" }
                });
            
            migrationBuilder.InsertData(
                table: "ContentBlock",
                columns: new[] { "Id", "ContentSectionId", "Order", "Type", "MarkDownBlock_Body" },
                values: new object[,]
                {
                    { new Guid("a0b85d7d-a9bd-48b5-82c6-a119adc74ca2"), new Guid("4f30b382-ce28-4a3e-801a-ce76004f5eb4"), 1, "MarkDownBlock", @"Read national statistical summaries, view charts and tables and download data files.

                Find out how and why these statistics are collected and published - [Pupil absence statistics: methodology](../methodology/pupil-absence-in-schools-in-england)." },
                    { new Guid("7934e93d-2e11-478d-ab0e-f799f15164bb"), new Guid("f599c2e2-f215-423a-beab-c5c6a0c2e5a9"), 1, "MarkDownBlock", @"Read national statistical summaries, view charts and tables and download data files.

                Find out how and why these statistics are collected and published - [Permanent and fixed-period exclusion statistics: methodology](../methodology/permanent-and-fixed-period-exclusions-in-england)" },
                    { new Guid("31c6b325-cbfa-4108-9956-cde7fa6a99ec"), new Guid("93ef0486-479f-4013-8012-a66ed01f1880"), 1, "MarkDownBlock", @"Read national statistical summaries, view charts and tables and download data files.

                Find out how and why these statistics are collected and published - [Secondary and primary school applications and offers: methodology](../methodology/secondary-and-primary-schools-applications-and-offers)" },
                    { new Guid("b9732ba9-8dc3-4fbc-9c9b-e504e4b58fb9"), new Guid("93ef0486-479f-4013-8012-a66ed01f1880"), 1, "MarkDownBlock", @" * pupils missed on average 8.2 school days
                 * overall and unauthorised absence rates up on 2015/16
                 * unauthorised absence rise due to higher rates of unauthorised holidays
                 * 10% of pupils persistently absent during 2016/17" },
                    { new Guid("db00f19a-96b7-47c9-84eb-92d6ace41434"), new Guid("601aadcc-be7d-4d3e-9154-c9eb64144692"), 1, "MarkDownBlock", @"* majority of applicants received a preferred offer
                * percentage of applicants receiving secondary first choice offers decreases as applications increase
                * slight proportional increase in applicants receiving primary first choice offer as applications decrease
                " },
                    { new Guid("8a108b91-ff08-4866-9566-cf03e33cd4ec"), new Guid("8abdae8f-4119-41ac-8efd-2229b7ea31da"), 1, "MarkDownBlock", @"* majority of applicants received a preferred offer
                * percentage of applicants receiving secondary first choice offers decreases as applications increase
                * slight proportional increase in applicants receiving primary first choice offer as applications decrease
                " }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("31c6b325-cbfa-4108-9956-cde7fa6a99ec"));

            migrationBuilder.DeleteData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("7934e93d-2e11-478d-ab0e-f799f15164bb"));

            migrationBuilder.DeleteData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("8a108b91-ff08-4866-9566-cf03e33cd4ec"));

            migrationBuilder.DeleteData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("a0b85d7d-a9bd-48b5-82c6-a119adc74ca2"));

            migrationBuilder.DeleteData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("b9732ba9-8dc3-4fbc-9c9b-e504e4b58fb9"));

            migrationBuilder.DeleteData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("db00f19a-96b7-47c9-84eb-92d6ace41434"));

            migrationBuilder.DeleteData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("c0241ab7-f40a-4755-bc69-365eba8114a3"));

            migrationBuilder.DeleteData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("4f30b382-ce28-4a3e-801a-ce76004f5eb4"));

            migrationBuilder.DeleteData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("601aadcc-be7d-4d3e-9154-c9eb64144692"));

            migrationBuilder.DeleteData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("8abdae8f-4119-41ac-8efd-2229b7ea31da"));

            migrationBuilder.DeleteData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("93ef0486-479f-4013-8012-a66ed01f1880"));

            migrationBuilder.DeleteData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("f599c2e2-f215-423a-beab-c5c6a0c2e5a9"));

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ContentSections");

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "ReleaseSummaryVersions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Releases",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
