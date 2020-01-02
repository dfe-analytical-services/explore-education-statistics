using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class UpdateContentSectionsAndBlocksToUseNewOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("038093a2-0be3-440b-8b22-8116e34aa616"),
                columns: new[] { "Order", "DataBlock_Charts", "CustomFootnotes", "DataBlock_Request", "DataBlock_Heading", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { 2, "[{\"Legend\":\"top\",\"Labels\":{\"181_461_____\":{\"Label\":\"Fixed period exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"181\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]", null, "{\"SubjectId\":12,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"461\"],\"Indicators\":[\"181\",\"177\",\"180\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "Chart showing fixed-period exclusions in England", null, "[{\"indicators\":[\"177\",\"180\",\"181\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                columns: new[] { "Order", "DataBlock_Charts", "CustomFootnotes", "DataBlock_Request", "Source", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { 1, "[{\"Legend\":\"top\",\"Labels\":{\"181_461_____\":{\"Label\":\"Fixed period exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"183_461_____\":{\"Label\":\"Pupils with one or more exclusion\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"181\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"183\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]", null, "{\"SubjectId\":12,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"461\"],\"Indicators\":[\"176\",\"177\",\"178\",\"179\",\"180\",\"181\",\"183\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, "{\"dataKeys\":[\"179\",\"181\",\"178\"],\"dataSummary\":[\"Up from 0.08% in 2015/16\",\"Up from 4.29% in 2015/16\",\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Number of permanent exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\",\"Number of fixed-period exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\",\"Total number of permanent exclusions within a school year. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"description\":null}", "[{\"indicators\":[\"179\",\"181\",\"178\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e"),
                columns: new[] { "Order", "CustomFootnotes", "DataBlock_Request", "Source", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { 1, null, "{\"SubjectId\":17,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"575\"],\"Indicators\":[\"211\",\"212\",\"216\",\"217\",\"218\",\"219\",\"220\",\"221\",\"222\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, "{\"dataKeys\":[\"212\",\"216\",\"217\"],\"dataSummary\":[\"Down from 620,330 in 2017\",\"Down from 558,411 in 2017\",\"Down from 34,792 in 2017\"],\"dataDefinition\":[\"Total number of applications received for places at primary and secondary schools.\",\"Total number of first preferences offered to applicants by schools.\",\"Total number of second preferences offered to applicants by schools.\"],\"description\":null}", "[{\"indicators\":[\"212\",\"211\",\"216\",\"217\",\"218\",\"221\",\"222\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4a1af98a-ed8a-438e-92d4-d21cca0429f9"),
                columns: new[] { "Order", "DataBlock_Charts", "CustomFootnotes", "DataBlock_Request" },
                values: new object[] { 2, "[{\"Labels\":{\"23_1_58_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"26_1_58_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"},\"28_1_58_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"23\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"26\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"28\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null}},\"Type\":\"map\",\"Title\":null,\"Width\":0,\"Height\":0}]", null, "{\"SubjectId\":1,\"GeographicLevel\":\"LocalAuthorityDistrict\",\"TimePeriod\":{\"StartYear\":\"2016\",\"StartCode\":\"AY\",\"EndYear\":\"2017\",\"EndCode\":\"AY\"},\"Filters\":[\"1\",\"58\"],\"Indicators\":[\"23\",\"26\",\"28\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("52916052-81e3-4b66-80b8-24f8666d9cbf"),
                columns: new[] { "Order", "CustomFootnotes", "DataBlock_Request", "DataBlock_Heading", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { 1, null, "{\"SubjectId\":17,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"577\"],\"Indicators\":[\"220\",\"221\",\"222\",\"223\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "Table of Timeseries of key secondary preference rates, England", null, "[{\"indicators\":[\"220\",\"221\",\"222\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                columns: new[] { "Order", "DataBlock_Charts", "CustomFootnotes", "DataBlock_Request", "Source", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { 1, "[{\"Legend\":\"top\",\"Labels\":{\"23_1_58_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"26_1_58_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"},\"28_1_58_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"23\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"26\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"28\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]", null, "{\"SubjectId\":1,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1\",\"58\"],\"Indicators\":[\"23\",\"26\",\"28\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, "{\"dataKeys\":[\"26\",\"28\",\"23\"],\"dataSummary\":[\"Up from 4.6% in 2015/16\",\"Similar to previous years\",\"Up from 1.1% in 2015/16\"],\"dataDefinition\":[\"Total number of all authorised and unauthorised absences from possible school sessions for all pupils. <a href=\\\"/glossary#overall-absence\\\">More >>></a>\",\"Number of authorised absences as a percentage of the overall school population. <a href=\\\"/glossary#authorised-absence\\\">More >>></a>\",\"Number of unauthorised absences as a percentage of the overall school population. <a href=\\\"/glossary#unauthorised-absence\\\">More >>></a>\"],\"description\":null}", "[{\"indicators\":[\"23\",\"26\",\"28\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"),
                columns: new[] { "Order", "DataBlock_Charts", "CustomFootnotes", "DataBlock_Request", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { 2, "[{\"Legend\":\"top\",\"Labels\":{\"23_1_58_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"26_1_58_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"},\"28_1_58_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"23\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"26\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"28\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]", null, "{\"SubjectId\":1,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1\",\"58\"],\"Indicators\":[\"23\",\"26\",\"28\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, "[{\"indicators\":[\"23\",\"26\",\"28\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("a8c408ed-45d8-4690-a9f3-2fb0e86377bf"),
                columns: new[] { "Order", "CustomFootnotes", "DataBlock_Request", "DataBlock_Heading", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { 2, null, "{\"SubjectId\":17,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"575\"],\"Indicators\":[\"220\",\"221\",\"222\",\"223\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "Table showing Timeseries of key primary preference rates, England Entry into academic year", null, "[{\"indicators\":[\"220\",\"221\",\"222\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("dd572e49-87e3-46f5-bb04-e9008573fc91"),
                columns: new[] { "Order", "DataBlock_Charts", "CustomFootnotes", "DataBlock_Request", "DataBlock_Heading", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { 3, "[{\"Legend\":\"top\",\"Labels\":{\"179_461_____\":{\"Label\":\"Fixed period exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"179\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Exclusion Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]", null, "{\"SubjectId\":12,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"461\"],\"Indicators\":[\"179\",\"177\",\"178\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "Chart showing permanent exclusions in England", null, "[{\"indicators\":[\"177\",\"178\",\"179\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("13e4577a-2291-4ce4-a8c9-6c76baa06092"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 2, @"**Secondary applications**

The number of applications received for secondary school places increased to 582,761 - up 3.6% since 2017. This follows a 2.6% increase between 2016 and 2017.

This continues the increase in secondary applications seen since 2013 which came on the back of a rise in births which began in the previous decade.

Since 2013, when secondary applications were at their lowest, there has been a 16.6% increase in the number of applications.

**Secondary offers**

The proportion of secondary applicants receiving an offer of their first-choice school has decreased to 82.1% - down from 83.5% in 2017.

The proportion of applicants who received an offer of any of their preferred schools also decreased slightly to 95.5% - down from 96.1% in 2017.

**Secondary National Offer Day**

These statistics come from the process undertaken by local authorities (LAs) which enabled them to send out offers of secondary school places to all applicants on the [Secondary National Offer Day](../glossary#national-offer-day) of 1 March 2018.

The secondary figures have been collected since 2008 and can be viewed as a time series in the following table." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("1a1d29f6-c4d5-41a9-9a06-b2ce84043edd"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"There's considerable variation in the [permanent exclusion](../glossary#permanent-exclusion) and [fixed-period exclusion](../glossary#fixed-period-exclusion) rate at the LA level.

**Permanent exclusion**

Similar to 2015/16, the regions with the joint-highest rates across all school types were:

* North West - 0.14%

* North West - 0.14%

Similar to 2015/16, the regions with the lowest rates were:

* South East - 0.06%

* Yorkshire and the Humber - 0.07%

**Fixed-period exclusion**

Similar to 2015/16, the region with the highest rates across all school types was Yorkshire and the Humber at 7.22% while the lowest rate was in Outer London (3.49%)." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("2c369594-3bbc-40b4-ad19-196c923f5c7f"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"**Overall absence**

The [overall absence](../glossary#overall-absence) rate has increased across state-funded primary, secondary and special schools between 2015/16 and 2016/17 driven by an increase in the unauthorised absence rate.

It increased from 4.6% to 4.7% over this period while the [unauthorised absence](../glossary#unauthorised-absence) rate increased from 1.1% to 1.3%.

The rate stayed the same at 4% in primary schools but increased from 5.2% to 5.4% for secondary schools. However, in special schools it was much higher and rose to 9.7%.

The overall and [authorised absence](../glossary#authorised-absence) rates have been fairly stable over recent years after gradually decreasing between 2006/07 and 2013/14." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("2ef5f84f-e151-425d-8906-2921712f9157"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"**Illness**

This is the main driver behind [overall absence](../glossary#overall-absence) and accounted for 55.3% of all absence - down from 57.3% in 2015/16 and 60.1% in 2014/15.

While the overall absence rate has slightly increased since 2015/16 the illness rate has stayed the same at 2.6%.

The absence rate due to other unauthorised circumstances has also stayed the same since 2015/16 at 0.7%.

**Absence due to family holiday**

The unauthorised holiday absence rate has increased gradually since 2006/07 while authorised holiday absence rates are much lower than in 2006/07 and remained steady over recent years.

The percentage of pupils who missed at least 1 session due to family holiday increased to 16.9% - up from 14.7% in 2015/16.

The absence rate due to family holidays agreed by the school stayed at 0.1%.

Meanwhile, the percentage of all possible sessions missed due to unauthorised family holidays increased to 0.4% - up from 0.3% in 2015/16.

**Regulation amendment**

A regulation amendment in September 2013 stated that term-time leave could only be granted in exceptional circumstances which explains the sharp fall in authorised holiday absence between 2012/13 and 2013/14.

These statistics and data relate to the period after the [Isle of Wight Council v Jon Platt High Court judgment (May 2016)](https://commonslibrary.parliament.uk/insights/term-time-holidays-supreme-court-judgment/) where the High Court supported a local magistrates’ ruling that there was no case to answer.

They also partially relate to the period after the April 2017 Supreme Court judgment where it unanimously agreed that no children should be taken out of school without good reason and clarified that 'regularly' means 'in accordance with the rules prescribed by the school'." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("33c3a82e-7d8d-47fc-9019-2fe5344ec32d"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 2, @"These have been broken down into the following:

* distribution of absence by reason - the proportion of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of absences

* rate of absence by reason - the rate of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of possible sessions

* one or more sessions missed due to each reason - the number of pupils missing at least 1 session due to each reason" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("3913a0af-9455-4802-a037-c4cfd4719b18"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 3, @"**Unauthorised absence**

The [unauthorised absence](../glossary#unauthorised-absence) rate has not varied much since 2006/07 but is at its highest since records began - 1.3%.

This is due to an increase in absence due to family holidays not agreed by schools.

**Authorised absence**

The [authorised absence](../glossary#authorised-absence) rate has stayed at 3.4% since 2015/16 but has been decreasing in recent years within primary schools.

**Total number of days missed**

The total number of days missed for [overall absence](../glossary#overall-absence) across state-funded primary, secondary and special schools has increased to 56.7 million from 54.8 million in 2015/16.

This partly reflects a rise in the total number of pupils with the average number of days missed per pupil slightly increased to 8.2 days from 8.1 days in 2015/16.

In 2016/17, 91.8% of primary, secondary and special school pupils missed at least 1 session during the school year - similar to the 91.7% figure from 2015/16." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("3aaafa20-bc32-4523-bb23-dd55c458f928"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"The [overall absence](../glossary#overall-absence) rate increased to 33.9% - up from 32.6% in 2015/16.

The [persistent absence](../glossary#persistent-absence) rate increased to 73.9% - up from 72.5% in 2015/16." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("49aa2ac2-1b65-4c25-9828-fec65a5ed7e8"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"The statistics and data cover the number of offers made to applicants for primary and secondary school places and the proportion which have received their preferred offers.

The data was collected from local authorities (LAs) where it was produced as part of the annual applications and offers process for applicants requiring a primary or secondary school place in September 2018.

The offers were made, and data collected, based on the National Offer Days of 1 March 2018 for secondary schools and 16 April 2018 for primary schools." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4aa06200-406b-4f5a-bee4-19e3b83eb1d2"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 2, @"**Persistent absentees**

The [overall absence](../glossary#overall-absence) rate for persistent absentees across all schools increased to 18.1% - nearly 4 times higher than the rate for all pupils. This is slightly up from 17.6% in 2015/16.

**Illness absence rate**

The illness absence rate is almost 4 times higher for persistent absentees at 7.6% compared to 2% for other pupils." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4e05bbb3-bd4e-4602-8424-069e59034c87"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"**Pupils with one or more fixed-period exclusion definition**

The number of pupils with [one or more fixed-period exclusion](../glossary#one-or-more-fixed-period-exclusion) has increased across state-funded primary, secondary and special schools to 183,475 (2.29% of pupils) up from 167,125 (2.11% of pupils) in 2015/16.

Of these kinds of pupils, 59.1% excluded on only 1 occasion while 1.5% received 10 or more fixed-period exclusions during the year.

The percentage of pupils who went on to receive a [permanent exclusion](../glossary#permanent-exclusion) was 3.5%.

The average length of [fixed-period exclusion](../glossary#fixed-period-exclusion) across schools decreased to 2.1 days - slightly shorter than in 2015/16.

The highest proportion of fixed-period exclusions (46.6%) lasted for only 1 day.

Only 2.0% of fixed-period exclusions lasted for longer than 1 week and longer fixed-period exclusions were more prevalent in secondary schools." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5f194c52-0ffb-4205-8c03-068ff4d1384b"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"**Primary applications**

The number of applications received for primary school places decreased to 608,180 - down 2% on 2017 (620,330).

This is the result of a notable fall in births since 2013 which is now feeding into primary school applications.

The number of primary applications is the lowest seen since 2013 - when this data was first collected.

**Primary offers**

The proportion of primary applicants receiving an offer of their first-choice school has increased to 91% - up from 90% in 2017.

The proportion of applicants who received an offer of any of their offer of any of their preferences has also increased slightly to 98.1% - up from 97.7% in 2017.

**Primary National Offer Day**

These statistics come from the process undertaken by local authorities (LAs) which enabled them to send out offers of primary school places to all applicants on the Primary National Offer Day of 16 April 2018.

The primary figures have been collected and published since 2014 and can be viewed as a time series in the following table." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("70546a7d-5edd-4b8f-b096-cfd50153f4cb"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"The number of [permanent exclusions](../glossary#permanent-exclusion) has increased across all state-funded primary, secondary and special schools to 7,720 - up from 6,685 in 2015/16.

This works out to an average 40.6 permanent exclusions per day - up from an 35.2 per day in 2015/16.

The permanent exclusion rate has also increased to 0.10% of pupils - up from from 0.08% in 2015/16 - which is equivalent to around 10 pupils per 10,000." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("7971329a-9e16-468b-9eb3-62bfc384b5a3"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"The number of fixed-period exclusionshas increased across all state-funded primary, secondary and special schools to 381,865 - up from 339,360 in 2015/16.

This works out to around 2,010 fixed-period exclusions per day - up from an 1,786 per day in 2015/16." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("7d97f8ed-e1d0-4244-bec3-3432af356f57"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"[Overall absence](../glossary#overall-absence) and [persistent absence](../glossary#persistent-absence) rates vary across primary, secondary and special schools by region and local authority (LA).

**Overall absence**

Similar to 2015/16, the 3 regions with the highest rates across all school types were:

* North East - 4.9%

* Yorkshire and the Humber - 4.9%

* South West - 4.8%

Meanwhile, Inner and Outer London had the lowest rates at 4.4%.

**Persistent absence**

The region with the highest persistent absence rate was Yorkshire and the Humber with 11.9% while Outer London had the lowest rate at 10%." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("7eeb1478-ab26-4b70-9128-b976429efa2f"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"The statistics and data cover the absence of pupils of compulsory school age during the 2016/17 academic year in the following state-funded school types:

- primary schools
- secondary schools
- special schools

They also includes information fo [pupil referral units](../glossary#pupil-referral-unit) and pupils aged 4 years.

We use the key measures of [overall absence](../glossary#overall-absence) and [persistent absence](../glossary#persistent-absence) to monitor pupil absence and also include [absence by reason](#contents-sections-heading-4) and [pupil characteristics](#contents-sections-heading-6).

The statistics and data are available at national, regional, local authority (LA) and school level and are used by LAs and schools to compare their local absence rates to regional and national averages for different pupil groups.

They're also used for policy development as key indicators in behaviour and school attendance policy." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("81d8eba2-9cba-4b04-bb02-e00ace5c4418"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 2, @"Most occurred in secondary schools which accounted for 83% of all permanent exclusions.

The [permanent exclusion](../glossary#permanent-exclusion) rate in secondary schools increased 0.20% - up from from 0.17% in 2015/16 - which is equivalent to 20 pupils per 10,000.

The rate also rose in primary schools to 0.03% but decreased in special schools to 0.07% - down from from 0.08% in 2015/16.

The rate generally followed a downward trend after 2006/07 - when it stood at 0.12%.

However, since 2012/13 it has been on the rise although rates are still lower now than in 2006/07." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("8510640f-d8b6-4fe2-a161-d025e14930a4"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"**First preference rates**

   At local authority (LA) level, the 3 highest first preference rates were achieved by the following local authorities:

   * Northumberland - 98.1%

   * East Riding of Yorkshire - 96.7%

   * Bedford - 96.4%

   Northumberland has been the top performer in this measure since 2015.

   As in previous years, the lowest first preference rates were all in London.

   * Hammersmith and Fulham - 51.4%

   * Kensington and Chelsea - 54.3%

   * Lambeth - 55.2%

   These figures do not include City of London which has a tiny number of applications and no secondary schools.

   Hammersmith and Fulham has had the lowest first preference rate since 2015.

   The higher number of practical options available to London applicants and ability to name 6 preferences may encourage parents to make more speculative choices for their top preferences.

   **Regional variation**

   There's much less regional variation in the proportions receiving any preferred offer compared to those for receiving a first preference as shown in the following chart." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("87f5343b-b7a5-4775-b483-d1668fac03fb"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 2, @"An applicant can apply for any school, including those situated in another local authority (LA).

Their authority liaises with the requested school (to make sure the applicant is considered under the admissions criteria) and makes the offer.

**Secondary offers**

In 2018, 91.6% of secondary offers made were from schools inside the home authority. This figure has been stable for the past few years.

This release concentrates on the headline figures for the proportion of children receiving their first preference or a preferred offer.

However, the main tables provide more information including:

* the number of places available

* the proportion of children for whom a preferred offer was not received

* whether applicants were provided with offers inside or outside their home authority" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("8a8add13-368c-4067-9210-166bb19a00c1"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"The [persistent absence](../glossary#persistent-absence) rate increased to and accounted for 37.6% of all absence - up from 36.6% in 2015 to 16 but still down from 43.3% in 2011 to 12.

It also accounted for almost a third (31.6%) of all [authorised absence](../glossary#authorised-absence) and more than half (53.8%) of all [unauthorised absence](../glossary#unauthorised-absence).

Overall, it's increased across primary and secondary schools to 10.8% - up from 10.5% in 2015 to 16." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("8e10ad6c-9a68-4162-84f9-81fb6dc93ae3"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 2, @"**Primary offers**
In 2018, 97.1% of primary offers made were from schools inside the home authority. This figure has been stable since 2014 when this data was first collected and published.

As in previous years, at primary level a smaller proportion of offers were made of schools outside the applicant’s home authority compared to secondary level." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("97c54e5f-2406-4333-851d-b6c9cc4bf612"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"The [overall absence](../glossary#overall-absence) rate decreased to 5.1% - down from 5.2% for the previous 2 years.

Absence recorded for 4-year-olds is not treated as authorised or unauthorised and only reported as overall absence." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("97d414f4-1a27-4ed7-85ea-c4c903e1d8cb"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"The statistics and data cover permanent and fixed period exclusions and school-level exclusions during the 2016/17 academic year in the following state-funded school types as reported in the school census:

* primary schools

* secondary schools

* special schools

They also include national-level information on permanent and fixed-period exclusions for [pupil referral units](../glossary#pupil-referral-unit).

All figures are based on unrounded data so constituent parts may not add up due to rounding." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("99d75d39-7ea5-456e-979d-1215fa673a83"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"All reasons (except bullying and theft) saw an increase in [permanent exclusions](../glossary#permanent-exclusion) since 2015/16.

The following most common reasons saw the largest increases:

* physical assault against a pupil

* persistent disruptive behaviour

* other reasons

**Persistent disruptive behaviour**

Remained the most common reason for permanent exclusions accounting for 2,755 (35.7%) of all permanent exclusions - which is equivalent to 3 permanent exclusions per 10,000 pupils.

However, in special schools the most common reason for exclusion was physical assault against an adult - accounting for 37.8% of all permanent exclusions and 28.1% of all [fixed-period exclusions](../glossary#fixed-period-exclusion).

Persistent disruptive behaviour is also the most common reason for fixed-period exclusions accounting for 108,640 %) of all fixed-period exclusions - up from 27.7% in 2015/16. This is equivalent to around 135 fixed-period exclusions per 10,000 pupils.

All reasons saw an increase in fixed-period exclusions since 2015/16. Persistent disruptive behaviour and other reasons saw the largest increases." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("c73382ce-73ff-465f-8f1b-7a08cb6af089"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"There was a similar pattern to previous years where the following groups (where higher exclusion rates are expected) showed an increase in exclusions since 2015/16:

* boys

* national curriculum years 9 and 10

* pupils with special educational needs (SEN)

* pupils known to be eligible for and claiming free school meals (FSM)

**Age, national curriculum year group and gender**

* more than half of all [permanent exclusions](../glossary#permanent-exclusion) (57.2%) and [fixed-period exclusions](../glossary#fixed-period-exclusion) (52.6 %) occur in national curriculum year 9 or above

* a quarter (25%) of all permanent exclusions were for pupils aged 14 - who also had the highest rates for fixed-period exclusion and pupils receiving [one or more fixed-period exclusion](../glossary#one-or-more-fixed-period-exclusion)

* the permanent exclusion rate for boys (0.15%) was more than 3 times higher than for girls (0.04%)

* the fixed-period exclusion rate for boys (6.91%) was almost 3 times higher than for girls (2.53%)

**Pupils eligible for and claiming free school meals (FSM)**

* had a permanent exclusion rate of 0.28% and fixed period exclusion rate of 12.54% - around 4 times higher than those not eligible for FSM at 0.07% and 3.50% respectively

* accounted for 40% of all permanent exclusions and 36.7% of all fixed-period exclusions

**Special educational needs (SEN) pupils**

* accounted for around half of all permanent exclusions (46.7%) and fixed-period exclusions (44.9%)

* had the highest permanent exclusion rate (0.35%0 - 6 times higher than the rate for pupils with no SEN (0.06%)

* pupils with a statement of SEN or education, health and care (EHC) plan had the highest fixed-period exclusion rate at 15.93% - more than 5 times higher than pupils with no SEN (3.06%)

**Ethnic group**

* pupils of Gypsy/Roma and Traveller of Irish Heritage ethnic groups had the highest rates of permanent and fixed-period exclusions - but as the population is relatively small these figures should be treated with some caution

* pupils from a Black Caribbean background had a permanent exclusion rate nearly 3 times higher (0.28%) than the school population as a whole (0.10%)

* pupils of Asian ethnic groups had the lowest permanent and fixed-period exclusion rates" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("cf01208f-cbab-41d1-9fa5-4793d2a0bc13"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"Nearly half of all pupils (48.9%) were absent for 5 days or less across primary, secondary and special schools - down from 49.1% in 2015/16.

The average total absence for primary school pupils was 7.2 days compared to 16.9 days for special school and 9.3 day for secondary school pupils.

The rate of pupils who had more than 25 days of absence stayed the same as in 2015/16 at 4.3%.

These pupils accounted for 23.5% of days missed while 8.2% of pupils had no absence.

**Absence by term**

Across all schools:

* [overall absence](../glossary#overall-absence) - highest in summer and lowest in autumn

* [authorised absence](../glossary#authorised-absence) - highest in spring and lowest in summer

* [unauthorised absence](../glossary#unauthorised-absence) - highest in summer" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("d3288340-2689-4346-91a6-c070e7b0799d"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"**Permanent exclusion**

The [permanent exclusion](../glossary#permanent-exclusion) rate in [pupil referral units](../glossary#pupil-referral-unit) decreased to 0.13 - down from 0.14% in 2015/16.

Permanent exclusions rates have remained fairly steady following an increase between 2013/14 and 2014/15.

**Fixed-period exclusion**

The [fixed period exclusion](../glossary#fixed-period-exclusion) rate has been steadily increasing since 2013/14.

The percentage of pupils in pupil referral units who 1 or more fixed-period exclusion increased to 59.17% - up from 58.15% in 2015/16." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("d988a5e8-4e3c-4c1d-b5a9-bf0e1d947085"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, "There were 560 reviews lodged with [independent review panels](../glossary#independent-review-panel) in maintained primary, secondary and special schools and academies of which 525 (93.4%) were determined and 45 (8.0%) resulted in an offer of reinstatement." });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("e4497a91-3e3b-460a-8965-42eab5e06ce5"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"**First preference rates**

At local authority (LA) level, the 3 highest first preference rates were achieved by the following local authorities:

* East Riding of Yorkshire - 97.6%

* Northumberland - 97.4%

* Rutland - 97.4%

These authorities are in the top 3 for the first time since 2015.

The lowest first preference rates were all in London.

* Kensington and Chelsea - 68.4%

* Camden - 76.5%

* Hammersmith and Fulham - 76.6%

Hammersmith and Fulham and Kensington and Chelsea have both been in the bottom 3 since 2015.

Although overall results are better at primary level than at secondary, for London as a whole the improvement is much more marked:

* primary first preference rate increased to 86.6% - up from 85.9% in 2017

* secondary first preference rate decreased to 66% - down from 68.% in 2017" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("e9462ed0-10dc-4ff5-8cda-f8c3b66f2714"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 3, @"**Primary schools**

* fixed-period exclusions numbers increased to 64,340 - up from 55,740 in 2015/16

* fixed-period exclusions rate increased to 1.37% - up from 1.21% in 2015/16

**Secondary schools**

* fixed-period exclusions numbers increased to 302,890 - up from 270,135 in 2015/16

* fixed-period exclusions rate increased to 9.4% - up from 8.46% in 2015/16

**Special schools**

* fixed-period exclusions numbers increased to 14,635 - up from 13,485 in 2015/16

* fixed-period exclusions rate increased to 13.03% - up from 12.53% in 2015/16" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("eb4318f9-11e0-46ea-9796-c36a9dc25014"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, @"The [overall absence](../glossary#overall-absence) and [persistent absence](../glossary#persistent-absence) patterns for pupils with different characteristics have been consistent over recent years.
**Ethnic groups**

Overall absence rate:

* Travellers of Irish heritage and Gypsy / Roma pupils - highest at 18.1% and 12.9% respectively

* Chinese and Black African ethnicity pupils - substantially lower than the national average of 4.7% at 2.4% and 2.9% respectively

Persistent absence rate:

* Travellers of Irish heritage pupils - highest at 64%

* Chinese pupils - lowest at 3.1%

**Free school meals (FSM) eligibility**

Overall absence rate:

* pupils known to be eligible for and claiming FSM - higher at 7.3% compared to 4.2% for non-FSM pupils

Persistent absence rate:

* pupils known to be eligible for and claiming FSM - more than double the rate of non-FSM pupils

**Gender**

Overall absence rate:

* boys and girls - very similar at 4.7% and 4.6% respectively

Persistent absence rate:

* boys and girls - similar at 10.9% and 10.6% respectively

**National curriculum year group**

Overall absence rate:

* pupils in national curriculum year groups 3 and 4 - lowest at 3.9% and 4% respectively

* pupils in national curriculum year groups 10 and 11 - highest at 6.1% and 6.2% respectively

This trend is repeated for the persistent absence rate.

**Special educational need (SEN)**

Overall absence rate:

* pupils with a SEN statement or education healthcare (EHC) plan - 8.2% compared to 4.3% for those with no identified SEN

Persistent absence rate:

* pupils with a SEN statement or education healthcare (EHC) plan - more than 2 times higher than pupils with no identified SEN" });


            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("8ab41234-cc9d-4b3d-a42c-c9fce7762719"),
                column: "Content",
                value: "[{\"Id\":\"d82b2a2c-b117-4f96-b812-80de5304ae21\",\"Order\":1,\"Heading\":\"1. Overview of applications and offers statistics\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"ff7b34e3-58ba-4578-849a-e5044fc14b8d\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"f0814433-92d4-4ce5-b63b-2f2cb1b6f48a\",\"Order\":2,\"Heading\":\"2. The admissions process\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"d7f310d7-e917-47e2-9e05-065c8bcab891\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"1d7a492b-3e59-4624-9a2a-076635d1f780\",\"Order\":3,\"Heading\":\"3. Methodology\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"a89226ef-1d6a-48ba-a795-0dbc334a9198\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"f129939b-803f-461b-8838-e7a3d8c6eca2\",\"Order\":4,\"Heading\":\"4. Contacts\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"16898320-cf69-45c4-8dbb-2486901759b1\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"}]");

            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("c8c911e3-39c1-452b-801f-25bb79d1deb7"),
                columns: new[] { "Annexes", "Content" },
                values: new object[] { "[{\"Id\":\"2bb1ce6d-8b54-4a77-bf7d-466c5f7f6bc3\",\"Order\":1,\"Heading\":\"Annex A - Calculations\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"c2d4d345-59b6-443b-bcb0-67c1f1dd9732\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"01e9feb8-8ca0-4d98-8a17-78672e4641a7\",\"Order\":2,\"Heading\":\"Annex B - Exclusion by reason codes\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"e4e4f98b-cbeb-451f-bd17-8e2d572b83f4\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"39576875-4a54-4028-bdb0-fecc67041f82\",\"Order\":3,\"Heading\":\"Annex C - Links to pupil exclusions statistics and data\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"94246f85-e43a-4b6c-97a0-b045701dc077\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"e3bfcc04-7d91-45b7-b0ee-19713de4b433\",\"Order\":4,\"Heading\":\"Annex D - Standard breakdowns\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"b05ae1f7-b2d1-4ae3-9db6-28fb0edf98ae\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"}]", "[{\"Id\":\"bceaafc1-9548-4a03-98d5-d3476c8b9d99\",\"Order\":1,\"Heading\":\"1. Overview of exclusion statistics\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"9a034a5f-7cdb-4895-b205-864e7f834ebf\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"66b15928-46c6-48d5-90e6-12cf354b4e04\",\"Order\":2,\"Heading\":\"2. National Statistics badging\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"f745893e-68a6-4813-ba8b-35d44c0935aa\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"863f2b02-67b1-41bd-b1c9-f998f4581297\",\"Order\":3,\"Heading\":\"3. Methodology\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"4c88cbdd-e0e2-4019-a656-31e4b97d19d5\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"fc66f72e-0176-4c75-b15f-2f35c7329563\",\"Order\":4,\"Heading\":\"4. Data collection\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"085ba061-918b-4e6e-9a02-3a8b12671587\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"0c44636a-9a31-4e05-8db7-331ed5eae366\",\"Order\":5,\"Heading\":\"5. Data processing\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"29f138eb-070e-41fe-95c6-e271cdf4eaf4\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"69df08b6-dcda-449e-828e-5666c8e6d533\",\"Order\":6,\"Heading\":\"6. Data quality\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"8263cdc1-a2e8-4db6-a581-44043a6add64\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"fa315759-a51b-4860-8ae5-7b9505873108\",\"Order\":7,\"Heading\":\"7. Contacts\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"e1eb03d9-25e4-4247-b3de-49805cce7889\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"}]" });

            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("caa8e56f-41d2-4129-a5c3-53b051134bd7"),
                columns: new[] { "Annexes", "Content" },
                values: new object[] { "[{\"Id\":\"0522bb29-1e0d-455a-88ef-5887f76fb069\",\"Order\":1,\"Heading\":\"Annex A - Calculations\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"8b90b3b2-f63d-4499-91aa-41ccae74e1c7\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"f1aac714-665d-436e-a488-1ca409d618bf\",\"Order\":2,\"Heading\":\"Annex B - School attendance codes\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"47f3e500-ec9f-4a00-96f8-c488f76b06e6\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"0b888133-215a-4b28-8c24-e0ee9a32df6e\",\"Order\":3,\"Heading\":\"Annex C - Links to pupil absence national statistics and data\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"a00a7765-aa81-43f2-afe1-fead7f070291\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"4c4c71e2-24e1-4b57-8a23-ce54fae9b329\",\"Order\":4,\"Heading\":\"Annex D - Standard breakdowns\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"7cc516d4-fc79-4e22-b35b-a042d5b14d35\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"97a138bf-4ebb-4b17-86ab-ed78584608e3\",\"Order\":5,\"Heading\":\"Annex E - Timeline\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"8ddc6877-acd2-479d-a86f-1139c1bd429f\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"dc00e749-0893-47f7-8440-5a4da47ceed7\",\"Order\":6,\"Heading\":\"Annex F - Absence rates over time\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"0fd71cfd-cfdd-42c5-86e7-9e311beee646\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"}]", "[{\"Id\":\"5a7fd947-d131-475d-afcd-11ab2b1ece67\",\"Order\":1,\"Heading\":\"1. Overview of absence statistics\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"4d5ae97d-fa1c-4a09-a0a3-b28307fcfb09\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"dabb7562-0433-42fc-96e4-64a68f399dac\",\"Order\":2,\"Heading\":\"2. National Statistics badging\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"6bf20dd4-a7d6-4bc6-a13a-9f574935c9af\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"50b5031a-93e4-4756-843e-21f88f52ba68\",\"Order\":3,\"Heading\":\"3. Methodology\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"63a318d9-05fa-40eb-9808-b825a6deb54a\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"e4ca520f-b609-4abb-a38c-c2d610a18e9f\",\"Order\":4,\"Heading\":\"4. Data collection\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"7714efb9-cc82-4895-ba27-bf5464541e38\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"da91d355-b878-4135-a0a9-fb538c601246\",\"Order\":5,\"Heading\":\"5. Data processing\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"6f81ab70-5730-4cf1-a513-669f5c4bef09\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"8df45966-5444-4487-be49-763c5009eea6\",\"Order\":6,\"Heading\":\"6. Data quality\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"a40d6c9e-fe61-48c0-b907-9757148beb0d\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"bf6870de-07d3-4e65-a877-373a63dbcc5d\",\"Order\":7,\"Heading\":\"7. Contacts\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"f620a229-21b7-4c6e-afd4-e9feb111f09a\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"}]" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
                        migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("038093a2-0be3-440b-8b22-8116e34aa616"),
                columns: new[] { "Order", "DataBlock_Charts", "CustomFootnotes", "DataBlock_Request", "DataBlock_Heading", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { 1, null, "{\"SubjectId\":12,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"461\"],\"Indicators\":[\"181\",\"177\",\"180\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "Chart showing fixed-period exclusions in England", null, "[{\"indicators\":[\"177\",\"180\",\"181\"],\"tableHeaders\":null}]", null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                columns: new[] { "Order", "DataBlock_Charts", "CustomFootnotes", "DataBlock_Request", "Source", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { 1, null, "{\"SubjectId\":12,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"461\"],\"Indicators\":[\"176\",\"177\",\"178\",\"179\",\"180\",\"181\",\"183\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, "{\"dataKeys\":[\"179\",\"181\",\"178\"],\"dataSummary\":[\"Up from 0.08% in 2015/16\",\"Up from 4.29% in 2015/16\",\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Number of permanent exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\",\"Number of fixed-period exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\",\"Total number of permanent exclusions within a school year. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"description\":{\"Body\":\" * overall permanent exclusions rate has increased to 0.10% - up from 0.08% in 2015/16\\n * number of exclusions increased to 7,720 - up from 6,685 in 2015/16\\n * overall fixed-period exclusions rate increased to 4.76% - up from 4.29% in 2015/16\\n * number of exclusions increased to 381,865 - up from 339,360 in 2015/16\\n\",\"Type\":\"MarkDownBlock\",\"Id\":\"132bef6e-c2a3-459d-996e-40f29ed6e74f\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":0}}", "[{\"indicators\":[\"179\",\"181\",\"178\"],\"tableHeaders\":null}]", null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e"),
                columns: new[] { "Order", "CustomFootnotes", "DataBlock_Request", "Source", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { 1, "{\"SubjectId\":17,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"575\"],\"Indicators\":[\"211\",\"212\",\"216\",\"217\",\"218\",\"219\",\"220\",\"221\",\"222\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, "{\"dataKeys\":[\"212\",\"216\",\"217\"],\"dataSummary\":[\"Down from 620,330 in 2017\",\"Down from 558,411 in 2017\",\"Down from 34,792 in 2017\"],\"dataDefinition\":[\"Total number of applications received for places at primary and secondary schools.\",\"Total number of first preferences offered to applicants by schools.\",\"Total number of second preferences offered to applicants by schools.\"],\"description\":{\"Body\":\"* majority of applicants received a preferred offer\\n* percentage of applicants receiving secondary first choice offers decreases as applications increase\\n* slight proportional increase in applicants receiving primary first choice offer as applications decrease\\n\",\"Type\":\"MarkDownBlock\",\"Id\":\"fdcac9d3-dab5-445d-9802-a8af0990efb2\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":0}}", "[{\"indicators\":[\"212\",\"211\",\"216\",\"217\",\"218\",\"221\",\"222\"],\"tableHeaders\":null}]", null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4a1af98a-ed8a-438e-92d4-d21cca0429f9"),
                columns: new[] { "Order", "DataBlock_Charts", "CustomFootnotes", "DataBlock_Request" },
                values: new object[] { 1, null, "{\"SubjectId\":1,\"GeographicLevel\":\"LocalAuthorityDistrict\",\"TimePeriod\":{\"StartYear\":\"2016\",\"StartCode\":\"AY\",\"EndYear\":\"2017\",\"EndCode\":\"AY\"},\"Filters\":[\"1\",\"58\"],\"Indicators\":[\"23\",\"26\",\"28\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("52916052-81e3-4b66-80b8-24f8666d9cbf"),
                columns: new[] { "Order", "CustomFootnotes", "DataBlock_Request", "DataBlock_Heading", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { 1, "{\"SubjectId\":17,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"577\"],\"Indicators\":[\"220\",\"221\",\"222\",\"223\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "Table of Timeseries of key secondary preference rates, England", null, "[{\"indicators\":[\"220\",\"221\",\"222\"],\"tableHeaders\":null}]", null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                columns: new[] { "Order", "DataBlock_Charts", "CustomFootnotes", "DataBlock_Request", "Source", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { 1, null, "{\"SubjectId\":1,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1\",\"58\"],\"Indicators\":[\"23\",\"26\",\"28\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, "{\"dataKeys\":[\"26\",\"28\",\"23\"],\"dataSummary\":[\"Up from 4.6% in 2015/16\",\"Similar to previous years\",\"Up from 1.1% in 2015/16\"],\"dataDefinition\":[\"Total number of all authorised and unauthorised absences from possible school sessions for all pupils. <a href=\\\"/glossary#overall-absence\\\">More >>></a>\",\"Number of authorised absences as a percentage of the overall school population. <a href=\\\"/glossary#authorised-absence\\\">More >>></a>\",\"Number of unauthorised absences as a percentage of the overall school population. <a href=\\\"/glossary#unauthorised-absence\\\">More >>></a>\"],\"description\":{\"Body\":\" * pupils missed on average 8.2 school days\\n * overall and unauthorised absence rates up on 2015/16\\n * unauthorised absence rise due to higher rates of unauthorised holidays\\n * 10% of pupils persistently absent during 2016/17\",\"Type\":\"MarkDownBlock\",\"Id\":\"f928762e-9bd5-4538-a4f0-d7f34b2874e6\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":0}}", "[{\"indicators\":[\"23\",\"26\",\"28\"],\"tableHeaders\":null}]", null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"),
                columns: new[] { "Order", "DataBlock_Charts", "CustomFootnotes", "DataBlock_Request", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { 1, null, "{\"SubjectId\":1,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1\",\"58\"],\"Indicators\":[\"23\",\"26\",\"28\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, "[{\"indicators\":[\"23\",\"26\",\"28\"],\"tableHeaders\":null}]", null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("a8c408ed-45d8-4690-a9f3-2fb0e86377bf"),
                columns: new[] { "Order", "CustomFootnotes", "DataBlock_Request", "DataBlock_Heading", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { 1, "{\"SubjectId\":17,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"575\"],\"Indicators\":[\"220\",\"221\",\"222\",\"223\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "Table showing Timeseries of key primary preference rates, England Entry into academic year", null, "[{\"indicators\":[\"220\",\"221\",\"222\"],\"tableHeaders\":null}]", null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("dd572e49-87e3-46f5-bb04-e9008573fc91"),
                columns: new[] { "Order", "DataBlock_Charts", "CustomFootnotes", "DataBlock_Request", "DataBlock_Heading", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { 1, null, "{\"SubjectId\":12,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"461\"],\"Indicators\":[\"179\",\"177\",\"178\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "Chart showing permanent exclusions in England", null, "[{\"indicators\":[\"177\",\"178\",\"179\"],\"tableHeaders\":null}]", null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("13e4577a-2291-4ce4-a8c9-6c76baa06092"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("1a1d29f6-c4d5-41a9-9a06-b2ce84043edd"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("2c369594-3bbc-40b4-ad19-196c923f5c7f"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("2ef5f84f-e151-425d-8906-2921712f9157"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("33c3a82e-7d8d-47fc-9019-2fe5344ec32d"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("3913a0af-9455-4802-a037-c4cfd4719b18"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("3aaafa20-bc32-4523-bb23-dd55c458f928"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("49aa2ac2-1b65-4c25-9828-fec65a5ed7e8"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4aa06200-406b-4f5a-bee4-19e3b83eb1d2"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4e05bbb3-bd4e-4602-8424-069e59034c87"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5f194c52-0ffb-4205-8c03-068ff4d1384b"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("70546a7d-5edd-4b8f-b096-cfd50153f4cb"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("7971329a-9e16-468b-9eb3-62bfc384b5a3"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("7d97f8ed-e1d0-4244-bec3-3432af356f57"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("7eeb1478-ab26-4b70-9128-b976429efa2f"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("81d8eba2-9cba-4b04-bb02-e00ace5c4418"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("8510640f-d8b6-4fe2-a161-d025e14930a4"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("87f5343b-b7a5-4775-b483-d1668fac03fb"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("8a8add13-368c-4067-9210-166bb19a00c1"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("8e10ad6c-9a68-4162-84f9-81fb6dc93ae3"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("97c54e5f-2406-4333-851d-b6c9cc4bf612"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("97d414f4-1a27-4ed7-85ea-c4c903e1d8cb"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("99d75d39-7ea5-456e-979d-1215fa673a83"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("c73382ce-73ff-465f-8f1b-7a08cb6af089"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("cf01208f-cbab-41d1-9fa5-4793d2a0bc13"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("d3288340-2689-4346-91a6-c070e7b0799d"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("d988a5e8-4e3c-4c1d-b5a9-bf0e1d947085"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("e4497a91-3e3b-460a-8965-42eab5e06ce5"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("e9462ed0-10dc-4ff5-8cda-f8c3b66f2714"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("eb4318f9-11e0-46ea-9796-c36a9dc25014"),
                columns: new[] { "Order", "MarkDownBlock_Body" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("8ab41234-cc9d-4b3d-a42c-c9fce7762719"),
                column: "Content",
                value: "[{\"Id\":\"d82b2a2c-b117-4f96-b812-80de5304ae21\",\"Order\":1,\"Heading\":\"1. Overview of applications and offers statistics\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"ff7b34e3-58ba-4578-849a-e5044fc14b8d\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"f0814433-92d4-4ce5-b63b-2f2cb1b6f48a\",\"Order\":2,\"Heading\":\"2. The admissions process\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"d7f310d7-e917-47e2-9e05-065c8bcab891\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"1d7a492b-3e59-4624-9a2a-076635d1f780\",\"Order\":3,\"Heading\":\"3. Methodology\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"a89226ef-1d6a-48ba-a795-0dbc334a9198\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"f129939b-803f-461b-8838-e7a3d8c6eca2\",\"Order\":4,\"Heading\":\"4. Contacts\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"16898320-cf69-45c4-8dbb-2486901759b1\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"}]");

            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("c8c911e3-39c1-452b-801f-25bb79d1deb7"),
                columns: new[] { "Annexes", "Content" },
                values: new object[] { "[{\"Id\":\"2bb1ce6d-8b54-4a77-bf7d-466c5f7f6bc3\",\"Order\":1,\"Heading\":\"Annex A - Calculations\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"c2d4d345-59b6-443b-bcb0-67c1f1dd9732\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"01e9feb8-8ca0-4d98-8a17-78672e4641a7\",\"Order\":2,\"Heading\":\"Annex B - Exclusion by reason codes\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"e4e4f98b-cbeb-451f-bd17-8e2d572b83f4\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"39576875-4a54-4028-bdb0-fecc67041f82\",\"Order\":3,\"Heading\":\"Annex C - Links to pupil exclusions statistics and data\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"94246f85-e43a-4b6c-97a0-b045701dc077\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"e3bfcc04-7d91-45b7-b0ee-19713de4b433\",\"Order\":4,\"Heading\":\"Annex D - Standard breakdowns\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"b05ae1f7-b2d1-4ae3-9db6-28fb0edf98ae\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"}]", "[{\"Id\":\"bceaafc1-9548-4a03-98d5-d3476c8b9d99\",\"Order\":1,\"Heading\":\"1. Overview of exclusion statistics\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"9a034a5f-7cdb-4895-b205-864e7f834ebf\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"66b15928-46c6-48d5-90e6-12cf354b4e04\",\"Order\":2,\"Heading\":\"2. National Statistics badging\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"f745893e-68a6-4813-ba8b-35d44c0935aa\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"863f2b02-67b1-41bd-b1c9-f998f4581297\",\"Order\":3,\"Heading\":\"3. Methodology\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"4c88cbdd-e0e2-4019-a656-31e4b97d19d5\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"fc66f72e-0176-4c75-b15f-2f35c7329563\",\"Order\":4,\"Heading\":\"4. Data collection\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"085ba061-918b-4e6e-9a02-3a8b12671587\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"0c44636a-9a31-4e05-8db7-331ed5eae366\",\"Order\":5,\"Heading\":\"5. Data processing\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"29f138eb-070e-41fe-95c6-e271cdf4eaf4\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"69df08b6-dcda-449e-828e-5666c8e6d533\",\"Order\":6,\"Heading\":\"6. Data quality\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"8263cdc1-a2e8-4db6-a581-44043a6add64\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"fa315759-a51b-4860-8ae5-7b9505873108\",\"Order\":7,\"Heading\":\"7. Contacts\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"e1eb03d9-25e4-4247-b3de-49805cce7889\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"}]" });

            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("caa8e56f-41d2-4129-a5c3-53b051134bd7"),
                columns: new[] { "Annexes", "Content" },
                values: new object[] { "[{\"Id\":\"0522bb29-1e0d-455a-88ef-5887f76fb069\",\"Order\":1,\"Heading\":\"Annex A - Calculations\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"8b90b3b2-f63d-4499-91aa-41ccae74e1c7\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"f1aac714-665d-436e-a488-1ca409d618bf\",\"Order\":2,\"Heading\":\"Annex B - School attendance codes\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"47f3e500-ec9f-4a00-96f8-c488f76b06e6\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"0b888133-215a-4b28-8c24-e0ee9a32df6e\",\"Order\":3,\"Heading\":\"Annex C - Links to pupil absence national statistics and data\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"a00a7765-aa81-43f2-afe1-fead7f070291\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"4c4c71e2-24e1-4b57-8a23-ce54fae9b329\",\"Order\":4,\"Heading\":\"Annex D - Standard breakdowns\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"7cc516d4-fc79-4e22-b35b-a042d5b14d35\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"97a138bf-4ebb-4b17-86ab-ed78584608e3\",\"Order\":5,\"Heading\":\"Annex E - Timeline\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"8ddc6877-acd2-479d-a86f-1139c1bd429f\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"dc00e749-0893-47f7-8440-5a4da47ceed7\",\"Order\":6,\"Heading\":\"Annex F - Absence rates over time\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"0fd71cfd-cfdd-42c5-86e7-9e311beee646\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"}]", "[{\"Id\":\"5a7fd947-d131-475d-afcd-11ab2b1ece67\",\"Order\":1,\"Heading\":\"1. Overview of absence statistics\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"4d5ae97d-fa1c-4a09-a0a3-b28307fcfb09\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"dabb7562-0433-42fc-96e4-64a68f399dac\",\"Order\":2,\"Heading\":\"2. National Statistics badging\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"6bf20dd4-a7d6-4bc6-a13a-9f574935c9af\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"50b5031a-93e4-4756-843e-21f88f52ba68\",\"Order\":3,\"Heading\":\"3. Methodology\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"63a318d9-05fa-40eb-9808-b825a6deb54a\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"e4ca520f-b609-4abb-a38c-c2d610a18e9f\",\"Order\":4,\"Heading\":\"4. Data collection\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"7714efb9-cc82-4895-ba27-bf5464541e38\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"da91d355-b878-4135-a0a9-fb538c601246\",\"Order\":5,\"Heading\":\"5. Data processing\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"6f81ab70-5730-4cf1-a513-669f5c4bef09\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"8df45966-5444-4487-be49-763c5009eea6\",\"Order\":6,\"Heading\":\"6. Data quality\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"a40d6c9e-fe61-48c0-b907-9757148beb0d\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"bf6870de-07d3-4e65-a877-373a63dbcc5d\",\"Order\":7,\"Heading\":\"7. Contacts\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"f620a229-21b7-4c6e-afd4-e9feb111f09a\",\"ContentSection\":null,\"ContentSectionId\":null,\"Order\":1}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"}]" });

            migrationBuilder.UpdateData(
                table: "ReleaseSummaryVersions",
                keyColumn: "Id",
                keyValue: new Guid("04adfe47-9057-4abd-a0e8-5a6ac56e1560"),
                column: "Summary",
                value: @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Permanent and fixed-period exclusion statistics: methodology](../methodology/permanent-and-fixed-period-exclusions-in-england)");

            migrationBuilder.UpdateData(
                table: "ReleaseSummaryVersions",
                keyColumn: "Id",
                keyValue: new Guid("420ca58e-278b-456b-9031-fe74a6966159"),
                column: "Summary",
                value: @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Pupil absence statistics: methodology](../methodology/pupil-absence-in-schools-in-england).");

            migrationBuilder.UpdateData(
                table: "ReleaseSummaryVersions",
                keyColumn: "Id",
                keyValue: new Guid("c6e08ed3-d93a-410a-9e7e-600f2cf25725"),
                column: "Summary",
                value: @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Secondary and primary school applications and offers: methodology](../methodology/secondary-and-primary-schools-applications-and-offers)");

            migrationBuilder.UpdateData(
                table: "ReleaseSummaryVersions",
                keyColumn: "Id",
                keyValue: new Guid("fe5e8cac-a574-4e83-861b-7b5f927d7d34"),
                column: "Summary",
                value: "Read national statistical summaries and definitions, view charts and tables and download data files across a range of pupil absence subject areas.");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "Summary",
                value: "Read national statistical summaries, view charts and tables and download data files.");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                column: "Summary",
                value: "Read national statistical summaries, view charts and tables and download data files.");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "Summary",
                value: "Read national statistical summaries, view charts and tables and download data files.");
        }
    }
}
