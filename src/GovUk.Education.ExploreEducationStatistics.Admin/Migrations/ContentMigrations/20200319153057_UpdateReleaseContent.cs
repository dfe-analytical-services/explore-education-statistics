using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class UpdateReleaseContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("33c3a82e-7d8d-47fc-9019-2fe5344ec32d"));

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("70546a7d-5edd-4b8f-b096-cfd50153f4cb"),
                column: "MarkDownBlock_Body",
                value: @"The number of [permanent exclusions](../glossary#permanent-exclusion) has increased across all state-funded primary, secondary and special schools to 7,720 - up from 6,685 in 2015/16.

This works out to an average 40.6 permanent exclusions per day - up from 35.2 per day in 2015/16.

The permanent exclusion rate has also increased to 0.10% of pupils - up from 0.08% in 2015/16 - which is equivalent to around 10 pupils per 10,000.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("7971329a-9e16-468b-9eb3-62bfc384b5a3"),
                column: "MarkDownBlock_Body",
                value: @"The number of fixed-period exclusionshas increased across all state-funded primary, secondary and special schools to 381,865 - up from 339,360 in 2015/16.

This works out to around 2,010 fixed-period exclusions per day - up from 1,786 per day in 2015/16.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("7d97f8ed-e1d0-4244-bec3-3432af356f57"),
                column: "MarkDownBlock_Body",
                value: @"[Overall absence](../glossary#overall-absence) and [persistent absence](../glossary#persistent-absence) rates vary across primary, secondary and special schools by region and local authority (LA).

**Overall absence**

Similar to 2015/16, the three regions with the highest rates across all school types were:

* North East - 4.9%

* Yorkshire and the Humber - 4.9%

* South West - 4.8%

Meanwhile, Inner and Outer London had the lowest rates at 4.4%.

**Persistent absence**

The region with the highest persistent absence rate was Yorkshire and the Humber with 11.9% while Outer London had the lowest rate at 10%.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("7eeb1478-ab26-4b70-9128-b976429efa2f"),
                column: "MarkDownBlock_Body",
                value: @"The statistics and data cover the absence of pupils of compulsory school age during the 2016/17 academic year in the following state-funded school types:

- primary schools
- secondary schools
- special schools

They also include information for [pupil referral units](../glossary#pupil-referral-unit) and pupils aged 4 years.

We use the key measures of [overall absence](../glossary#overall-absence) and [persistent absence](../glossary#persistent-absence) to monitor pupil absence and also include [absence by reason](#contents-sections-heading-4) and [pupil characteristics](#contents-sections-heading-6).

The statistics and data are available at national, regional, local authority (LA) and school level and are used by LAs and schools to compare their local absence rates to regional and national averages for different pupil groups.

They're also used for policy development as key indicators in behaviour and school attendance policy.

Within this release, absence by reason is broken down in three different ways:

* distribution of absence by reason - the proportion of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of absences

* rate of absence by reason - the rate of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of possible sessions

* one or more sessions missed due to each reason - the number of pupils missing at least 1 session due to each reason");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("81d8eba2-9cba-4b04-bb02-e00ace5c4418"),
                column: "MarkDownBlock_Body",
                value: @"Most occurred in secondary schools which accounted for 83% of all permanent exclusions.

The [permanent exclusion](../glossary#permanent-exclusion) rate in secondary schools increased 0.20% - up from 0.17% in 2015/16 - which is equivalent to 20 pupils per 10,000.

The rate also rose in primary schools to 0.03% but decreased in special schools to 0.07% - down from 0.08% in 2015/16.

The rate generally followed a downward trend after 2006/07 - when it stood at 0.12%.

However, since 2012/13 it has been on the rise although rates are still lower now than in 2006/07.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("8a8add13-368c-4067-9210-166bb19a00c1"),
                column: "MarkDownBlock_Body",
                value: @"The [persistent absence](../glossary#persistent-absence) rate increased to and accounted for 37.6% of all absence - up from 36.6% in 2015/16 but still down from 43.3% in 2011/12.

It also accounted for almost a third (31.6%) of all [authorised absence](../glossary#authorised-absence) and more than half (53.8%) of all [unauthorised absence](../glossary#unauthorised-absence).

Overall, it's increased across primary and secondary schools to 10.8% - up from 10.5% in 2015/16.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("97c54e5f-2406-4333-851d-b6c9cc4bf612"),
                column: "MarkDownBlock_Body",
                value: @"The [overall absence](../glossary#overall-absence) rate decreased to 5.1% - down from 5.2% for the previous two years.

Absence recorded for 4-year-olds is not treated as authorised or unauthorised and only reported as overall absence.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("99d75d39-7ea5-456e-979d-1215fa673a83"),
                column: "MarkDownBlock_Body",
                value: @"All reasons (except bullying and theft) saw an increase in [permanent exclusions](../glossary#permanent-exclusion) since 2015/16.

The following most common reasons saw the largest increases:

* physical assault against a pupil

* persistent disruptive behaviour

* other reasons

**Persistent disruptive behaviour**

Remained the most common reason for permanent exclusions accounting for 2,755 (35.7%) of all permanent exclusions - which is equivalent to 3 permanent exclusions per 10,000 pupils.

However, in special schools the most common reason for exclusion was physical assault against an adult - accounting for 37.8% of all permanent exclusions and 28.1% of all [fixed-period exclusions](../glossary#fixed-period-exclusion).

Persistent disruptive behaviour is also the most common reason for fixed-period exclusions accounting for 108,640 of all fixed-period exclusions - up from 27.7% in 2015/16. This is equivalent to around 135 fixed-period exclusions per 10,000 pupils.

All reasons saw an increase in fixed-period exclusions since 2015/16. Persistent disruptive behaviour and other reasons saw the largest increases.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("b9732ba9-8dc3-4fbc-9c9b-e504e4b58fb9"),
                columns: new[] { "ContentSectionId", "MarkDownBlock_Body" },
                values: new object[] { new Guid("c0241ab7-f40a-4755-bc69-365eba8114a3"), @"* pupils missed on average 8.2 school days
* overall and unauthorised absence rates up on 2015/16
* unauthorised absence rise due to higher rates of unauthorised holidays
* 10% of pupils persistently absent during 2016/17
" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("db00f19a-96b7-47c9-84eb-92d6ace41434"),
                column: "MarkDownBlock_Body",
                value: @"* The rate of permanent exclusions has increased since last year from 0.08 per cent of pupil enrolments in 2015/16 to 0.10 per cent in 2016/17. The number of exclusions has also increased, from 6,685 to 7,720.
* The rate of fixed period exclusions have also increased since last year from 4.29 per cent of pupil enrolments in 2015/16 to 4.76 per cent in 2016/17. The number of exclusions has also increased, from 339,360 to 381,865.
* There were 183,475 pupil enrolments, 2.29 per cent, with at least one fixed term exclusion in 2016/17, up from 167,125 pupil enrolments, 2.11 per cent, in 2015/16.
");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                column: "TimePeriodCoverage",
                value: "CY");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("70546a7d-5edd-4b8f-b096-cfd50153f4cb"),
                column: "MarkDownBlock_Body",
                value: @"The number of [permanent exclusions](../glossary#permanent-exclusion) has increased across all state-funded primary, secondary and special schools to 7,720 - up from 6,685 in 2015/16.

This works out to an average 40.6 permanent exclusions per day - up from an 35.2 per day in 2015/16.

The permanent exclusion rate has also increased to 0.10% of pupils - up from from 0.08% in 2015/16 - which is equivalent to around 10 pupils per 10,000.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("7971329a-9e16-468b-9eb3-62bfc384b5a3"),
                column: "MarkDownBlock_Body",
                value: @"The number of fixed-period exclusionshas increased across all state-funded primary, secondary and special schools to 381,865 - up from 339,360 in 2015/16.

This works out to around 2,010 fixed-period exclusions per day - up from an 1,786 per day in 2015/16.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("7d97f8ed-e1d0-4244-bec3-3432af356f57"),
                column: "MarkDownBlock_Body",
                value: @"[Overall absence](../glossary#overall-absence) and [persistent absence](../glossary#persistent-absence) rates vary across primary, secondary and special schools by region and local authority (LA).

**Overall absence**

Similar to 2015/16, the 3 regions with the highest rates across all school types were:

* North East - 4.9%

* Yorkshire and the Humber - 4.9%

* South West - 4.8%

Meanwhile, Inner and Outer London had the lowest rates at 4.4%.

**Persistent absence**

The region with the highest persistent absence rate was Yorkshire and the Humber with 11.9% while Outer London had the lowest rate at 10%.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("7eeb1478-ab26-4b70-9128-b976429efa2f"),
                column: "MarkDownBlock_Body",
                value: @"The statistics and data cover the absence of pupils of compulsory school age during the 2016/17 academic year in the following state-funded school types:

- primary schools
- secondary schools
- special schools

They also includes information fo [pupil referral units](../glossary#pupil-referral-unit) and pupils aged 4 years.

We use the key measures of [overall absence](../glossary#overall-absence) and [persistent absence](../glossary#persistent-absence) to monitor pupil absence and also include [absence by reason](#contents-sections-heading-4) and [pupil characteristics](#contents-sections-heading-6).

The statistics and data are available at national, regional, local authority (LA) and school level and are used by LAs and schools to compare their local absence rates to regional and national averages for different pupil groups.

They're also used for policy development as key indicators in behaviour and school attendance policy.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("81d8eba2-9cba-4b04-bb02-e00ace5c4418"),
                column: "MarkDownBlock_Body",
                value: @"Most occurred in secondary schools which accounted for 83% of all permanent exclusions.

The [permanent exclusion](../glossary#permanent-exclusion) rate in secondary schools increased 0.20% - up from from 0.17% in 2015/16 - which is equivalent to 20 pupils per 10,000.

The rate also rose in primary schools to 0.03% but decreased in special schools to 0.07% - down from from 0.08% in 2015/16.

The rate generally followed a downward trend after 2006/07 - when it stood at 0.12%.

However, since 2012/13 it has been on the rise although rates are still lower now than in 2006/07.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("8a8add13-368c-4067-9210-166bb19a00c1"),
                column: "MarkDownBlock_Body",
                value: @"The [persistent absence](../glossary#persistent-absence) rate increased to and accounted for 37.6% of all absence - up from 36.6% in 2015 to 16 but still down from 43.3% in 2011 to 12.

It also accounted for almost a third (31.6%) of all [authorised absence](../glossary#authorised-absence) and more than half (53.8%) of all [unauthorised absence](../glossary#unauthorised-absence).

Overall, it's increased across primary and secondary schools to 10.8% - up from 10.5% in 2015 to 16.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("97c54e5f-2406-4333-851d-b6c9cc4bf612"),
                column: "MarkDownBlock_Body",
                value: @"The [overall absence](../glossary#overall-absence) rate decreased to 5.1% - down from 5.2% for the previous 2 years.

Absence recorded for 4-year-olds is not treated as authorised or unauthorised and only reported as overall absence.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("99d75d39-7ea5-456e-979d-1215fa673a83"),
                column: "MarkDownBlock_Body",
                value: @"All reasons (except bullying and theft) saw an increase in [permanent exclusions](../glossary#permanent-exclusion) since 2015/16.

The following most common reasons saw the largest increases:

* physical assault against a pupil

* persistent disruptive behaviour

* other reasons

**Persistent disruptive behaviour**

Remained the most common reason for permanent exclusions accounting for 2,755 (35.7%) of all permanent exclusions - which is equivalent to 3 permanent exclusions per 10,000 pupils.

However, in special schools the most common reason for exclusion was physical assault against an adult - accounting for 37.8% of all permanent exclusions and 28.1% of all [fixed-period exclusions](../glossary#fixed-period-exclusion).

Persistent disruptive behaviour is also the most common reason for fixed-period exclusions accounting for 108,640 %) of all fixed-period exclusions - up from 27.7% in 2015/16. This is equivalent to around 135 fixed-period exclusions per 10,000 pupils.

All reasons saw an increase in fixed-period exclusions since 2015/16. Persistent disruptive behaviour and other reasons saw the largest increases.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("b9732ba9-8dc3-4fbc-9c9b-e504e4b58fb9"),
                columns: new[] { "ContentSectionId", "MarkDownBlock_Body" },
                values: new object[] { new Guid("93ef0486-479f-4013-8012-a66ed01f1880"), @" * pupils missed on average 8.2 school days
 * overall and unauthorised absence rates up on 2015/16
 * unauthorised absence rise due to higher rates of unauthorised holidays
 * 10% of pupils persistently absent during 2016/17" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("db00f19a-96b7-47c9-84eb-92d6ace41434"),
                column: "MarkDownBlock_Body",
                value: @"* majority of applicants received a preferred offer
* percentage of applicants receiving secondary first choice offers decreases as applications increase
* slight proportional increase in applicants receiving primary first choice offer as applications decrease
");

            migrationBuilder.InsertData(
                table: "ContentBlock",
                columns: new[] { "Id", "ContentSectionId", "Order", "Type", "MarkDownBlock_Body" },
                values: new object[] { new Guid("33c3a82e-7d8d-47fc-9019-2fe5344ec32d"), new Guid("fbf99442-3b72-46bc-836d-8866c552c53d"), 0, "MarkDownBlock", @"These have been broken down into the following:

* distribution of absence by reason - the proportion of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of absences

* rate of absence by reason - the rate of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of possible sessions

* one or more sessions missed due to each reason - the number of pupils missing at least 1 session due to each reason" });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                column: "TimePeriodCoverage",
                value: "AY");
        }
    }
}
