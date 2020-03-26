using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class MoreContentUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("038093a2-0be3-440b-8b22-8116e34aa616"),
                column: "DataBlock_Charts",
                value: "[{\"Legend\":\"top\",\"Labels\":{\"68aeda43-2b6a-433a-4c06-08d78f90080f_1f3f86a4-de9f-43d7-5bfd-08d78f900a85_____\":{\"Label\":\"Fixed period exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":[{\"Indicator\":\"68aeda43-2b6a-433a-4c06-08d78f90080f\",\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"Unit\":null,\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"Unit\":null,\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":5,\"Size\":null,\"TickConfig\":\"custom\",\"TickSpacing\":1}},\"Type\":\"line\",\"Title\":null,\"Width\":null,\"Height\":0,\"LegendHeight\":0}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("0b4c43cd-fc12-4159-88b9-0c8646424555"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"dataSummary\":[\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Total number of permanent exclusions within a school year.\"],\"dataDefinitionTitle\":[\"What is number of permanent exclusions?\"]}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17251e1c-e978-419c-98f5-963131c952f7"),
                columns: new[] { "DataBlock_Request", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"TimePeriod\":{\"StartYear\":2016,\"StartCode\":\"AY\",\"EndYear\":2016,\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"BoundaryLevel\":null,\"GeographicLevel\":null,\"Indicators\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null,\"PlanningArea\":null,\"IncludeGeoJson\":true}", "{\"dataKeys\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"dataSummary\":[\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Total number of permanent exclusions within a school year.\"],\"dataDefinitionTitle\":[\"What is number of permanent exclusions?\"]}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Number of permanent exclusions\",\"value\":\"167f4807-4fdd-461a-4c03-08d78f90080f\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"be3b765b-005f-4279-4c04-08d78f90080f\",\"68aeda43-2b6a-433a-4c06-08d78f90080f\",\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"dataSummary\":[\"Up from 0.08% in 2015/16\",\"Up from 4.29% in 2015/16\",\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Number of permanent exclusions as a percentage of the overall school population.\",\"Number of fixed-period exclusions as a percentage of the overall school population.\",\"Total number of permanent exclusions within a school year.\"],\"dataDefinitionTitle\":[\"What is permanent exclusion rate?\",\"What is fixed period exclusion rate?\",\"What is number of permanent exclusions?\"]}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("1869d10a-ca3f-450c-9685-780b11d916f5"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"dataSummary\":[\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Total number of permanent exclusions within a school year.\"],\"dataDefinitionTitle\":[\"What is number of permanent exclusions?\"]}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"f9ae4976-7cd3-4718-834a-09349b6eb377\",\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"],\"dataSummary\":[\"Up from 4.6% in 2015/16\",\"Similar to previous years\",\"Up from 1.1% in 2015/16\"],\"dataDefinition\":[\"Total number of all authorised and unauthorised absences from possible school sessions for all pupils.\",\"Number of authorised absences as a percentage of the overall school population.\",\"Number of unauthorised absences as a percentage of the overall school population.\"],\"dataDefinitionTitle\":[\"What is overall absence?\",\"What is authorized absence?\",\"What is unauthorized absence?\"]}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("695de169-947f-4f66-8564-6392b6113dfc"),
                columns: new[] { "DataBlock_Request", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"TimePeriod\":{\"StartYear\":2016,\"StartCode\":\"AY\",\"EndYear\":2016,\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"BoundaryLevel\":null,\"GeographicLevel\":null,\"Indicators\":[\"68aeda43-2b6a-433a-4c06-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null,\"PlanningArea\":null,\"IncludeGeoJson\":true}", "{\"dataKeys\":[\"68aeda43-2b6a-433a-4c06-08d78f90080f\"],\"dataSummary\":[\"Up from 4.29% in 2015/16\"],\"dataDefinition\":[\"Number of fixed-period exclusions as a percentage of the overall school population.\"],\"dataDefinitionTitle\":[\"What is fixed period exclusion rate?\"]}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Fixed period exclusion rate\",\"value\":\"68aeda43-2b6a-433a-4c06-08d78f90080f\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("d0397918-1697-40d8-b649-bea3c63c7d3e"),
                columns: new[] { "DataBlock_Request", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"TimePeriod\":{\"StartYear\":2016,\"StartCode\":\"AY\",\"EndYear\":2016,\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"BoundaryLevel\":null,\"GeographicLevel\":null,\"Indicators\":[\"be3b765b-005f-4279-4c04-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null,\"PlanningArea\":null,\"IncludeGeoJson\":true}", "{\"dataKeys\":[\"be3b765b-005f-4279-4c04-08d78f90080f\"],\"dataSummary\":[\"Up from 0.08% in 2015/16\"],\"dataDefinition\":[\"Number of permanent exclusions as a percentage of the overall school population.\"],\"dataDefinitionTitle\":[\"What is permanent exclusion rate?\"]}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Permanent exclusion rate\",\"value\":\"be3b765b-005f-4279-4c04-08d78f90080f\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("7934e93d-2e11-478d-ab0e-f799f15164bb"),
                column: "MarkDownBlock_Body",
                value: @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Permanent and fixed-period exclusion statistics: methodology](../methodology/permanent-and-fixed-period-exclusions-in-england)

This release was created as example content during the platform’s Private Beta phase, whilst it provides access to real data, the below release should be used with some caution. To access the original release, please see [Permanent and fixed-period exclusions in England: 2016 to 2017](https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2016-to-2017)");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("a0b85d7d-a9bd-48b5-82c6-a119adc74ca2"),
                column: "MarkDownBlock_Body",
                value: @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Pupil absence statistics: methodology](../methodology/pupil-absence-in-schools-in-england).

This release was created as example content during the platform’s Private Beta phase, whilst it provides access to real data, the below release should be used with some caution. To access the original, release please see [Pupil absence in schools in England: 2016 to 2017](https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2016-to-2017)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("038093a2-0be3-440b-8b22-8116e34aa616"),
                column: "DataBlock_Charts",
                value: "[{\"Legend\":\"top\",\"Labels\":{\"68aeda43-2b6a-433a-4c06-08d78f90080f_1f3f86a4-de9f-43d7-5bfd-08d78f900a85_____\":{\"Label\":\"Fixed period exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":[{\"Indicator\":\"68aeda43-2b6a-433a-4c06-08d78f90080f\",\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"Unit\":null,\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"Unit\":null,\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":null,\"Height\":0,\"LegendHeight\":0}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("0b4c43cd-fc12-4159-88b9-0c8646424555"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"dataSummary\":[\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Total number of permanent exclusions within a school year. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is number of permanent exclusions?\"]}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17251e1c-e978-419c-98f5-963131c952f7"),
                columns: new[] { "DataBlock_Request", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"TimePeriod\":{\"StartYear\":2012,\"StartCode\":\"AY\",\"EndYear\":2016,\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"BoundaryLevel\":null,\"GeographicLevel\":null,\"Indicators\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null,\"PlanningArea\":null,\"IncludeGeoJson\":true}", "{\"dataKeys\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"dataSummary\":[\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Total number of permanent exclusions within a school year. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is number of permanent exclusions?\"]}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Number of permanent exclusions\",\"value\":\"167f4807-4fdd-461a-4c03-08d78f90080f\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"be3b765b-005f-4279-4c04-08d78f90080f\",\"68aeda43-2b6a-433a-4c06-08d78f90080f\",\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"dataSummary\":[\"Up from 0.08% in 2015/16\",\"Up from 4.29% in 2015/16\",\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Number of permanent exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\",\"Number of fixed-period exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\",\"Total number of permanent exclusions within a school year. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is permanent exclusion rate?\",\"What is fixed period exclusion rate?\",\"What is number of permanent exclusions?\"]}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("1869d10a-ca3f-450c-9685-780b11d916f5"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"dataSummary\":[\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Total number of permanent exclusions within a school year. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is number of permanent exclusions?\"]}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"f9ae4976-7cd3-4718-834a-09349b6eb377\",\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"],\"dataSummary\":[\"Up from 4.6% in 2015/16\",\"Similar to previous years\",\"Up from 1.1% in 2015/16\"],\"dataDefinition\":[\"Total number of all authorised and unauthorised absences from possible school sessions for all pupils. <a href=\\\"/glossary#overall-absence\\\">More >>></a>\",\"Number of authorised absences as a percentage of the overall school population. <a href=\\\"/glossary#authorised-absence\\\">More >>></a>\",\"Number of unauthorised absences as a percentage of the overall school population. <a href=\\\"/glossary#unauthorised-absence\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is overall absence?\",\"What is authorized absence?\",\"What is unauthorized absence?\"]}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("695de169-947f-4f66-8564-6392b6113dfc"),
                columns: new[] { "DataBlock_Request", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"TimePeriod\":{\"StartYear\":2012,\"StartCode\":\"AY\",\"EndYear\":2016,\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"BoundaryLevel\":null,\"GeographicLevel\":null,\"Indicators\":[\"68aeda43-2b6a-433a-4c06-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null,\"PlanningArea\":null,\"IncludeGeoJson\":true}", "{\"dataKeys\":[\"68aeda43-2b6a-433a-4c06-08d78f90080f\"],\"dataSummary\":[\"Up from 4.29% in 2015/16\"],\"dataDefinition\":[\"Number of fixed-period exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is fixed period exclusion rate?\"]}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Fixed period exclusion rate\",\"value\":\"68aeda43-2b6a-433a-4c06-08d78f90080f\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("d0397918-1697-40d8-b649-bea3c63c7d3e"),
                columns: new[] { "DataBlock_Request", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"TimePeriod\":{\"StartYear\":2012,\"StartCode\":\"AY\",\"EndYear\":2016,\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"BoundaryLevel\":null,\"GeographicLevel\":null,\"Indicators\":[\"be3b765b-005f-4279-4c04-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null,\"PlanningArea\":null,\"IncludeGeoJson\":true}", "{\"dataKeys\":[\"be3b765b-005f-4279-4c04-08d78f90080f\"],\"dataSummary\":[\"Up from 0.08% in 2015/16\"],\"dataDefinition\":[\"Number of permanent exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is permanent exclusion rate?\"]}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Permanent exclusion rate\",\"value\":\"be3b765b-005f-4279-4c04-08d78f90080f\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("7934e93d-2e11-478d-ab0e-f799f15164bb"),
                column: "MarkDownBlock_Body",
                value: @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Permanent and fixed-period exclusion statistics: methodology](../methodology/permanent-and-fixed-period-exclusions-in-england)");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("a0b85d7d-a9bd-48b5-82c6-a119adc74ca2"),
                column: "MarkDownBlock_Body",
                value: @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Pupil absence statistics: methodology](../methodology/pupil-absence-in-schools-in-england).");
        }
    }
}
