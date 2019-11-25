using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class BreakUpKeyStatisticsDataBlocks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                column: "ContentSectionId",
                value: new Guid("c0241ab7-f40a-4755-bc69-365eba8114a3"));

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                column: "ContentSectionId",
                value: new Guid("601aadcc-be7d-4d3e-9154-c9eb64144692"));

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e"),
                column: "ContentSectionId",
                value: new Guid("8abdae8f-4119-41ac-8efd-2229b7ea31da"));

            migrationBuilder.InsertData(
                table: "ContentSections",
                columns: new[] { "Id", "Caption", "Heading", "Order", "ReleaseId", "Type" },
                values: new object[,]
                {
                    { new Guid("de8f8547-cbae-4d52-88ec-d78d0ad836ae"), "", "", 1, new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), "KeyStatistics" },
                    { new Guid("991a436a-9c7a-418b-ab06-60f2610b4bc6"), "", "", 1, new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), "KeyStatistics" },
                    { new Guid("7b779d79-6caa-43fd-84ba-b8efd219b3c8"), "", "", 1, new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), "KeyStatistics" }
                });

            migrationBuilder.InsertData(
                table: "ContentBlock",
                columns: new[] { "Id", "ContentSectionId", "Order", "Type", "DataBlock_Charts", "CustomFootnotes", "DataBlock_Request", "DataBlock_Heading", "Name", "Source", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[,]
                {
                    { new Guid("9ccb0daf-91a1-4cb0-b3c1-2aed452338bc"), new Guid("7b779d79-6caa-43fd-84ba-b8efd219b3c8"), 1, "DataBlock", "[{\"Legend\":\"top\",\"Labels\":{\"26_1_58_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"23\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"26\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"28\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]", null, "{\"SubjectId\":1,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1\",\"58\"],\"Indicators\":[\"26\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, null, null, "{\"dataKeys\":[\"26\"],\"dataSummary\":[\"Up from 4.6% in 2015/16\"],\"dataDefinition\":[\"Total number of all authorised and unauthorised absences from possible school sessions for all pupils. <a href=\\\"/glossary#overall-absence\\\">More >>></a>\"],\"description\":null}", "[{\"indicators\":[\"26\"],\"tableHeaders\":null}]" },
                    { new Guid("3da30a08-9eeb-4a99-9872-796c3ea518fa"), new Guid("7b779d79-6caa-43fd-84ba-b8efd219b3c8"), 2, "DataBlock", "[{\"Legend\":\"top\",\"Labels\":{\"28_1_58_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"28\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]", null, "{\"SubjectId\":1,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1\",\"58\"],\"Indicators\":[\"28\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, null, null, "{\"dataKeys\":[\"28\"],\"dataSummary\":[\"Similar to previous years\"],\"dataDefinition\":[\"Number of authorised absences as a percentage of the overall school population. <a href=\\\"/glossary#authorised-absence\\\">More >>></a>\"],\"description\":null}", "[{\"indicators\":[\"28\"],\"tableHeaders\":null}]" },
                    { new Guid("045a9585-688f-46fa-b3a9-9bdc237e0381"), new Guid("7b779d79-6caa-43fd-84ba-b8efd219b3c8"), 3, "DataBlock", "[{\"Legend\":\"top\",\"Labels\":{\"23_1_58_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"23\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"26\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"28\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]", null, "{\"SubjectId\":1,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1\",\"58\"],\"Indicators\":[\"23\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, null, null, "{\"dataKeys\":[\"23\"],\"dataSummary\":[\"Up from 1.1% in 2015/16\"],\"dataDefinition\":[\"Number of unauthorised absences as a percentage of the overall school population. <a href=\\\"/glossary#unauthorised-absence\\\">More >>></a>\"],\"description\":null}", "[{\"indicators\":[\"23\"],\"tableHeaders\":null}]" },
                    { new Guid("d0397918-1697-40d8-b649-bea3c63c7d3e"), new Guid("991a436a-9c7a-418b-ab06-60f2610b4bc6"), 1, "DataBlock", "[{\"Legend\":\"top\",\"Labels\":{\"179_461_____\":{\"Label\":\"Permanent exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"181\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"183\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]", null, "{\"SubjectId\":12,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"461\"],\"Indicators\":[\"179\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, null, null, "{\"dataKeys\":[\"179\"],\"dataSummary\":[\"Up from 0.08% in 2015/16\"],\"dataDefinition\":[\"Number of permanent exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"description\":null}", "[{\"indicators\":[\"179\"],\"tableHeaders\":null}]" },
                    { new Guid("695de169-947f-4f66-8564-6392b6113dfc"), new Guid("991a436a-9c7a-418b-ab06-60f2610b4bc6"), 2, "DataBlock", "[{\"Legend\":\"top\",\"Labels\":{\"181_461_____\":{\"Label\":\"Fixed period exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"181\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]", null, "{\"SubjectId\":12,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"461\"],\"Indicators\":[\"181\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, null, null, "{\"dataKeys\":[\"181\"],\"dataSummary\":[\"Up from 4.29% in 2015/16\"],\"dataDefinition\":[\"Number of fixed-period exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"description\":null}", "[{\"indicators\":[\"181\"],\"tableHeaders\":null}]" },
                    { new Guid("17251e1c-e978-419c-98f5-963131c952f7"), new Guid("991a436a-9c7a-418b-ab06-60f2610b4bc6"), 3, "DataBlock", "[{\"Legend\":\"top\",\"Labels\":{\"178_461_____\":{\"Label\":\"Number of permanent exclusions\",\"Value\":null,\"Name\":null,\"Unit\":\"\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"178\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]", null, "{\"SubjectId\":12,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"461\"],\"Indicators\":[\"178\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, null, null, "{\"dataKeys\":[\"178\"],\"dataSummary\":[\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Total number of permanent exclusions within a school year. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"description\":null}", "[{\"indicators\":[\"178\"],\"tableHeaders\":null}]" },
                    { new Guid("5947759d-c6f3-451b-b353-a4da063f020a"), new Guid("de8f8547-cbae-4d52-88ec-d78d0ad836ae"), 1, "DataBlock", null, null, "{\"SubjectId\":17,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"575\"],\"Indicators\":[\"212\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, null, null, "{\"dataKeys\":[\"212\"],\"dataSummary\":[\"Down from 620,330 in 2017\"],\"dataDefinition\":[\"Total number of applications received for places at primary and secondary schools.\"],\"description\":null}", "[{\"indicators\":[\"212\"],\"tableHeaders\":null}]" },
                    { new Guid("02a637e7-6cc7-44e5-8991-8982edfe49fc"), new Guid("de8f8547-cbae-4d52-88ec-d78d0ad836ae"), 2, "DataBlock", null, null, "{\"SubjectId\":17,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"575\"],\"Indicators\":[\"216\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, null, null, "{\"dataKeys\":[\"216\"],\"dataSummary\":[\"Down from 558,411 in 2017\"],\"dataDefinition\":[\"Total number of first preferences offered to applicants by schools.\"],\"description\":null}", "[{\"indicators\":[\"216\"],\"tableHeaders\":null}]" },
                    { new Guid("5d5f9b1f-8d0d-47d4-ba2b-ea97413d3117"), new Guid("de8f8547-cbae-4d52-88ec-d78d0ad836ae"), 3, "DataBlock", null, null, "{\"SubjectId\":17,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"575\"],\"Indicators\":[\"217\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, null, null, "{\"dataKeys\":[\"217\"],\"dataSummary\":[\"Down from 34,792 in 2017\"],\"dataDefinition\":[\"Total number of second preferences offered to applicants by schools.\"],\"description\":null}", "[{\"indicators\":[\"217\"],\"tableHeaders\":null}]" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("02a637e7-6cc7-44e5-8991-8982edfe49fc"));

            migrationBuilder.DeleteData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("045a9585-688f-46fa-b3a9-9bdc237e0381"));

            migrationBuilder.DeleteData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17251e1c-e978-419c-98f5-963131c952f7"));

            migrationBuilder.DeleteData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("3da30a08-9eeb-4a99-9872-796c3ea518fa"));

            migrationBuilder.DeleteData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5947759d-c6f3-451b-b353-a4da063f020a"));

            migrationBuilder.DeleteData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d5f9b1f-8d0d-47d4-ba2b-ea97413d3117"));

            migrationBuilder.DeleteData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("695de169-947f-4f66-8564-6392b6113dfc"));

            migrationBuilder.DeleteData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("9ccb0daf-91a1-4cb0-b3c1-2aed452338bc"));

            migrationBuilder.DeleteData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("d0397918-1697-40d8-b649-bea3c63c7d3e"));

            migrationBuilder.DeleteData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("7b779d79-6caa-43fd-84ba-b8efd219b3c8"));

            migrationBuilder.DeleteData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("991a436a-9c7a-418b-ab06-60f2610b4bc6"));

            migrationBuilder.DeleteData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("de8f8547-cbae-4d52-88ec-d78d0ad836ae"));

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                column: "ContentSectionId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                column: "ContentSectionId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e"),
                column: "ContentSectionId",
                value: null);
        }
    }
}
