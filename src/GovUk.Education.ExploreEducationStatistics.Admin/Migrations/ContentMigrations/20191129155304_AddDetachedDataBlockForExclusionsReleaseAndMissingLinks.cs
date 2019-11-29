using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class AddDetachedDataBlockForExclusionsReleaseAndMissingLinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ContentBlock",
                columns: new[] { "Id", "ContentSectionId", "Order", "Type", "DataBlock_Charts", "CustomFootnotes", "DataBlock_Request", "DataBlock_Heading", "Name", "Source", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { new Guid("1869d10a-ca3f-450c-9685-780b11d916f5"), null, 0, "DataBlock", "[{\"Legend\":\"top\",\"Labels\":{\"178_461_____\":{\"Label\":\"Number of permanent exclusions\",\"Value\":null,\"Name\":null,\"Unit\":\"\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"178\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]", null, "{\"SubjectId\":12,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"461\"],\"Indicators\":[\"178\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, "Available Data Block", null, "{\"dataKeys\":[\"178\"],\"dataSummary\":[\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Total number of permanent exclusions within a school year. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"description\":null}", "[{\"indicators\":[\"178\"],\"tableHeaders\":null}]" });

            migrationBuilder.InsertData(
                table: "ReleaseContentBlocks",
                columns: new[] { "ReleaseId", "ContentBlockId" },
                values: new object[,]
                {
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("4a1af98a-ed8a-438e-92d4-d21cca0429f9") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("038093a2-0be3-440b-8b22-8116e34aa616") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("1869d10a-ca3f-450c-9685-780b11d916f5") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("dd572e49-87e3-46f5-bb04-e9008573fc91") },
                    { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("52916052-81e3-4b66-80b8-24f8666d9cbf") },
                    { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("a8c408ed-45d8-4690-a9f3-2fb0e86377bf") }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ReleaseContentBlocks",
                keyColumns: new[] { "ReleaseId", "ContentBlockId" },
                keyValues: new object[] { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("4a1af98a-ed8a-438e-92d4-d21cca0429f9") });

            migrationBuilder.DeleteData(
                table: "ReleaseContentBlocks",
                keyColumns: new[] { "ReleaseId", "ContentBlockId" },
                keyValues: new object[] { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef") });

            migrationBuilder.DeleteData(
                table: "ReleaseContentBlocks",
                keyColumns: new[] { "ReleaseId", "ContentBlockId" },
                keyValues: new object[] { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("52916052-81e3-4b66-80b8-24f8666d9cbf") });

            migrationBuilder.DeleteData(
                table: "ReleaseContentBlocks",
                keyColumns: new[] { "ReleaseId", "ContentBlockId" },
                keyValues: new object[] { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("a8c408ed-45d8-4690-a9f3-2fb0e86377bf") });

            migrationBuilder.DeleteData(
                table: "ReleaseContentBlocks",
                keyColumns: new[] { "ReleaseId", "ContentBlockId" },
                keyValues: new object[] { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("038093a2-0be3-440b-8b22-8116e34aa616") });

            migrationBuilder.DeleteData(
                table: "ReleaseContentBlocks",
                keyColumns: new[] { "ReleaseId", "ContentBlockId" },
                keyValues: new object[] { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("1869d10a-ca3f-450c-9685-780b11d916f5") });

            migrationBuilder.DeleteData(
                table: "ReleaseContentBlocks",
                keyColumns: new[] { "ReleaseId", "ContentBlockId" },
                keyValues: new object[] { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("dd572e49-87e3-46f5-bb04-e9008573fc91") });

            migrationBuilder.DeleteData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("1869d10a-ca3f-450c-9685-780b11d916f5"));
        }
    }
}
