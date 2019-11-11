using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class UpdateChartDataBlock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("038093a2-0be3-440b-8b22-8116e34aa616"),
                columns: new[] { "DataBlock_Charts", "DataBlock_Request" },
                values: new object[] { "[{\"Legend\":\"top\",\"Labels\":{\"181_461_____\":{\"Label\":\"Fixed period exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":[{\"Indicator\":\"181\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]", "{\"SubjectId\":12,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"461\"],\"Indicators\":[\"181\",\"177\",\"180\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                columns: new[] { "DataBlock_Charts", "DataBlock_Request" },
                values: new object[] { "[{\"Legend\":\"top\",\"Labels\":{\"181_461_____\":{\"Label\":\"Fixed period exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"183_461_____\":{\"Label\":\"Pupils with one or more exclusion\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":[{\"Indicator\":\"181\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"183\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]", "{\"SubjectId\":12,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"461\"],\"Indicators\":[\"176\",\"177\",\"178\",\"179\",\"180\",\"181\",\"183\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e"),
                column: "DataBlock_Request",
                value: "{\"SubjectId\":17,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"575\"],\"Indicators\":[\"211\",\"212\",\"216\",\"217\",\"218\",\"219\",\"220\",\"221\",\"222\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4a1af98a-ed8a-438e-92d4-d21cca0429f9"),
                columns: new[] { "DataBlock_Charts", "DataBlock_Request" },
                values: new object[] { "[{\"Labels\":{\"23_1_58_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"26_1_58_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"},\"28_1_58_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":[{\"Indicator\":\"23\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"26\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"28\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null}},\"Type\":\"map\",\"Title\":null,\"Width\":0,\"Height\":0}]", "{\"SubjectId\":1,\"GeographicLevel\":\"Local_Authority_District\",\"TimePeriod\":{\"StartYear\":\"2016\",\"StartCode\":\"AY\",\"EndYear\":\"2017\",\"EndCode\":\"AY\"},\"Filters\":[\"1\",\"58\"],\"Indicators\":[\"23\",\"26\",\"28\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("52916052-81e3-4b66-80b8-24f8666d9cbf"),
                column: "DataBlock_Request",
                value: "{\"SubjectId\":17,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"577\"],\"Indicators\":[\"220\",\"221\",\"222\",\"223\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                columns: new[] { "DataBlock_Charts", "DataBlock_Request" },
                values: new object[] { "[{\"Legend\":\"top\",\"Labels\":{\"23_1_58_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"26_1_58_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"},\"28_1_58_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":[{\"Indicator\":\"23\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"26\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"28\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]", "{\"SubjectId\":1,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1\",\"58\"],\"Indicators\":[\"23\",\"26\",\"28\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"),
                columns: new[] { "DataBlock_Charts", "DataBlock_Request" },
                values: new object[] { "[{\"Legend\":\"top\",\"Labels\":{\"23_1_58_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"26_1_58_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"},\"28_1_58_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":[{\"Indicator\":\"23\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"26\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"28\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]", "{\"SubjectId\":1,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1\",\"58\"],\"Indicators\":[\"23\",\"26\",\"28\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("a8c408ed-45d8-4690-a9f3-2fb0e86377bf"),
                column: "DataBlock_Request",
                value: "{\"SubjectId\":17,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"575\"],\"Indicators\":[\"220\",\"221\",\"222\",\"223\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("dd572e49-87e3-46f5-bb04-e9008573fc91"),
                columns: new[] { "DataBlock_Charts", "DataBlock_Request" },
                values: new object[] { "[{\"Legend\":\"top\",\"Labels\":{\"179_461_____\":{\"Label\":\"Fixed period exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":[{\"Indicator\":\"179\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Exclusion Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]", "{\"SubjectId\":12,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"461\"],\"Indicators\":[\"179\",\"177\",\"178\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("038093a2-0be3-440b-8b22-8116e34aa616"),
                columns: new[] { "DataBlock_Charts", "DataBlock_Request" },
                values: new object[] { "[{\"Width\":0,\"Height\":0,\"Legend\":\"top\",\"Labels\":{\"181_461_____\":{\"Label\":\"Fixed period exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":[{\"Indicator\":\"181\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\"}]", "{\"SubjectId\":12,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"461\"],\"Indicators\":[\"181\",\"177\",\"180\"],\"Country\":null,\"LocalAuthority\":null,\"Region\":null}" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                columns: new[] { "DataBlock_Charts", "DataBlock_Request" },
                values: new object[] { "[{\"Width\":0,\"Height\":0,\"Legend\":\"top\",\"Labels\":{\"181_461_____\":{\"Label\":\"Fixed period exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"183_461_____\":{\"Label\":\"Pupils with one or more exclusion\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":[{\"Indicator\":\"181\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"183\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\"}]", "{\"SubjectId\":12,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"461\"],\"Indicators\":[\"176\",\"177\",\"178\",\"179\",\"180\",\"181\",\"183\"],\"Country\":null,\"LocalAuthority\":null,\"Region\":null}" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e"),
                column: "DataBlock_Request",
                value: "{\"SubjectId\":17,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"575\"],\"Indicators\":[\"211\",\"212\",\"216\",\"217\",\"218\",\"219\",\"220\",\"221\",\"222\"],\"Country\":null,\"LocalAuthority\":null,\"Region\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4a1af98a-ed8a-438e-92d4-d21cca0429f9"),
                columns: new[] { "DataBlock_Charts", "DataBlock_Request" },
                values: new object[] { "[{\"Width\":0,\"Height\":0,\"Labels\":{\"23_1_58_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"26_1_58_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"},\"28_1_58_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":[{\"Indicator\":\"23\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"26\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"28\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null}},\"Type\":\"map\"}]", "{\"SubjectId\":1,\"GeographicLevel\":\"Local_Authority_District\",\"TimePeriod\":{\"StartYear\":\"2016\",\"StartCode\":\"AY\",\"EndYear\":\"2017\",\"EndCode\":\"AY\"},\"Filters\":[\"1\",\"58\"],\"Indicators\":[\"23\",\"26\",\"28\"],\"Country\":null,\"LocalAuthority\":null,\"Region\":null}" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("52916052-81e3-4b66-80b8-24f8666d9cbf"),
                column: "DataBlock_Request",
                value: "{\"SubjectId\":17,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"577\"],\"Indicators\":[\"220\",\"221\",\"222\",\"223\"],\"Country\":null,\"LocalAuthority\":null,\"Region\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                columns: new[] { "DataBlock_Charts", "DataBlock_Request" },
                values: new object[] { "[{\"Width\":0,\"Height\":0,\"Legend\":\"top\",\"Labels\":{\"23_1_58_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"26_1_58_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"},\"28_1_58_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":[{\"Indicator\":\"23\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"26\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"28\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\"}]", "{\"SubjectId\":1,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1\",\"58\"],\"Indicators\":[\"23\",\"26\",\"28\"],\"Country\":null,\"LocalAuthority\":null,\"Region\":null}" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"),
                columns: new[] { "DataBlock_Charts", "DataBlock_Request" },
                values: new object[] { "[{\"Width\":0,\"Height\":0,\"Legend\":\"top\",\"Labels\":{\"23_1_58_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"26_1_58_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"},\"28_1_58_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":[{\"Indicator\":\"23\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"26\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"28\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\"}]", "{\"SubjectId\":1,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1\",\"58\"],\"Indicators\":[\"23\",\"26\",\"28\"],\"Country\":null,\"LocalAuthority\":null,\"Region\":null}" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("a8c408ed-45d8-4690-a9f3-2fb0e86377bf"),
                column: "DataBlock_Request",
                value: "{\"SubjectId\":17,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"575\"],\"Indicators\":[\"220\",\"221\",\"222\",\"223\"],\"Country\":null,\"LocalAuthority\":null,\"Region\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("dd572e49-87e3-46f5-bb04-e9008573fc91"),
                columns: new[] { "DataBlock_Charts", "DataBlock_Request" },
                values: new object[] { "[{\"Width\":0,\"Height\":0,\"Legend\":\"top\",\"Labels\":{\"179_461_____\":{\"Label\":\"Fixed period exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":[{\"Indicator\":\"179\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Exclusion Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\"}]", "{\"SubjectId\":12,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"461\"],\"Indicators\":[\"179\",\"177\",\"178\"],\"Country\":null,\"LocalAuthority\":null,\"Region\":null}" });
        }
    }
}
