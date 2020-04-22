﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class RemoveRelativeLinksFromContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("13e4577a-2291-4ce4-a8c9-6c76baa06092"),
                column: "MarkDownBlock_Body",
                value: @"**Secondary applications**

The number of applications received for secondary school places increased to 582,761 - up 3.6% since 2017. This follows a 2.6% increase between 2016 and 2017.

This continues the increase in secondary applications seen since 2013 which came on the back of a rise in births which began in the previous decade.

Since 2013, when secondary applications were at their lowest, there has been a 16.6% increase in the number of applications.

**Secondary offers**

The proportion of secondary applicants receiving an offer of their first-choice school has decreased to 82.1% - down from 83.5% in 2017.

The proportion of applicants who received an offer of any of their preferred schools also decreased slightly to 95.5% - down from 96.1% in 2017.

**Secondary National Offer Day**

These statistics come from the process undertaken by local authorities (LAs) which enabled them to send out offers of secondary school places to all applicants on the [Secondary National Offer Day](/glossary#national-offer-day) of 1 March 2018.

The secondary figures have been collected since 2008 and can be viewed as a time series in the following table.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("1a1d29f6-c4d5-41a9-9a06-b2ce84043edd"),
                column: "MarkDownBlock_Body",
                value: @"There's considerable variation in the [permanent exclusion](/glossary#permanent-exclusion) and [fixed-period exclusion](/glossary#fixed-period-exclusion) rate at the LA level.

**Permanent exclusion**

Similar to 2015/16, the regions with the joint-highest rates across all school types were:

* North West - 0.14%

* North West - 0.14%

Similar to 2015/16, the regions with the lowest rates were:

* South East - 0.06%

* Yorkshire and the Humber - 0.07%

**Fixed-period exclusion**

Similar to 2015/16, the region with the highest rates across all school types was Yorkshire and the Humber at 7.22% while the lowest rate was in Outer London (3.49%).");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("2c369594-3bbc-40b4-ad19-196c923f5c7f"),
                column: "MarkDownBlock_Body",
                value: @"**Overall absence**

The [overall absence](/glossary#overall-absence) rate has increased across state-funded primary, secondary and special schools between 2015/16 and 2016/17 driven by an increase in the unauthorised absence rate.

It increased from 4.6% to 4.7% over this period while the [unauthorised absence](/glossary#unauthorised-absence) rate increased from 1.1% to 1.3%.

The rate stayed the same at 4% in primary schools but increased from 5.2% to 5.4% for secondary schools. However, in special schools it was much higher and rose to 9.7%.

The overall and [authorised absence](/glossary#authorised-absence) rates have been fairly stable over recent years after gradually decreasing between 2006/07 and 2013/14.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("2ef5f84f-e151-425d-8906-2921712f9157"),
                column: "MarkDownBlock_Body",
                value: @"**Illness**

This is the main driver behind [overall absence](/glossary#overall-absence) and accounted for 55.3% of all absence - down from 57.3% in 2015/16 and 60.1% in 2014/15.

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

They also partially relate to the period after the April 2017 Supreme Court judgment where it unanimously agreed that no children should be taken out of school without good reason and clarified that 'regularly' means 'in accordance with the rules prescribed by the school'.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("31c6b325-cbfa-4108-9956-cde7fa6a99ec"),
                column: "MarkDownBlock_Body",
                value: @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Secondary and primary school applications and offers: methodology](/methodology/secondary-and-primary-schools-applications-and-offers)");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("3913a0af-9455-4802-a037-c4cfd4719b18"),
                column: "MarkDownBlock_Body",
                value: @"**Unauthorised absence**

The [unauthorised absence](/glossary#unauthorised-absence) rate has not varied much since 2006/07 but is at its highest since records began - 1.3%.

This is due to an increase in absence due to family holidays not agreed by schools.

**Authorised absence**

The [authorised absence](/glossary#authorised-absence) rate has stayed at 3.4% since 2015/16 but has been decreasing in recent years within primary schools.

**Total number of days missed**

The total number of days missed for [overall absence](/glossary#overall-absence) across state-funded primary, secondary and special schools has increased to 56.7 million from 54.8 million in 2015/16.

This partly reflects a rise in the total number of pupils with the average number of days missed per pupil slightly increased to 8.2 days from 8.1 days in 2015/16.

In 2016/17, 91.8% of primary, secondary and special school pupils missed at least 1 session during the school year - similar to the 91.7% figure from 2015/16.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("3aaafa20-bc32-4523-bb23-dd55c458f928"),
                column: "MarkDownBlock_Body",
                value: @"The [overall absence](/glossary#overall-absence) rate increased to 33.9% - up from 32.6% in 2015/16.

The [persistent absence](/glossary#persistent-absence) rate increased to 73.9% - up from 72.5% in 2015/16.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4aa06200-406b-4f5a-bee4-19e3b83eb1d2"),
                column: "MarkDownBlock_Body",
                value: @"**Persistent absentees**

The [overall absence](/glossary#overall-absence) rate for persistent absentees across all schools increased to 18.1% - nearly 4 times higher than the rate for all pupils. This is slightly up from 17.6% in 2015/16.

**Illness absence rate**

The illness absence rate is almost 4 times higher for persistent absentees at 7.6% compared to 2% for other pupils.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4e05bbb3-bd4e-4602-8424-069e59034c87"),
                column: "MarkDownBlock_Body",
                value: @"**Pupils with one or more fixed-period exclusion definition**

The number of pupils with [one or more fixed-period exclusion](/glossary#one-or-more-fixed-period-exclusion) has increased across state-funded primary, secondary and special schools to 183,475 (2.29% of pupils) up from 167,125 (2.11% of pupils) in 2015/16.

Of these kinds of pupils, 59.1% excluded on only 1 occasion while 1.5% received 10 or more fixed-period exclusions during the year.

The percentage of pupils who went on to receive a [permanent exclusion](/glossary#permanent-exclusion) was 3.5%.

The average length of [fixed-period exclusion](/glossary#fixed-period-exclusion) across schools decreased to 2.1 days - slightly shorter than in 2015/16.

The highest proportion of fixed-period exclusions (46.6%) lasted for only 1 day.

Only 2.0% of fixed-period exclusions lasted for longer than 1 week and longer fixed-period exclusions were more prevalent in secondary schools.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("70546a7d-5edd-4b8f-b096-cfd50153f4cb"),
                column: "MarkDownBlock_Body",
                value: @"The number of [permanent exclusions](/glossary#permanent-exclusion) has increased across all state-funded primary, secondary and special schools to 7,720 - up from 6,685 in 2015/16.

This works out to an average 40.6 permanent exclusions per day - up from 35.2 per day in 2015/16.

The permanent exclusion rate has also increased to 0.10% of pupils - up from 0.08% in 2015/16 - which is equivalent to around 10 pupils per 10,000.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("7934e93d-2e11-478d-ab0e-f799f15164bb"),
                column: "MarkDownBlock_Body",
                value: @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Permanent and fixed-period exclusion statistics: methodology](/methodology/permanent-and-fixed-period-exclusions-in-england)

This release was created as example content during the platform’s Private Beta phase, whilst it provides access to real data, the below release should be used with some caution. To access the original release, please see [Permanent and fixed-period exclusions in England: 2016 to 2017](https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2016-to-2017)");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("7971329a-9e16-468b-9eb3-62bfc384b5a3"),
                column: "MarkDownBlock_Body",
                value: @"The number of fixed-period exclusions has increased across all state-funded primary, secondary and special schools to 381,865 - up from 339,360 in 2015/16.

This works out to around 2,010 fixed-period exclusions per day - up from 1,786 per day in 2015/16.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("7d97f8ed-e1d0-4244-bec3-3432af356f57"),
                column: "MarkDownBlock_Body",
                value: @"[Overall absence](/glossary#overall-absence) and [persistent absence](/glossary#persistent-absence) rates vary across primary, secondary and special schools by region and local authority (LA).

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

They also include information for [pupil referral units](/glossary#pupil-referral-unit) and pupils aged 4 years.

We use the key measures of [overall absence](/glossary#overall-absence) and [persistent absence](/glossary#persistent-absence) to monitor pupil absence and also include [absence by reason](#contents-sections-heading-4) and [pupil characteristics](#contents-sections-heading-6).

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

The [permanent exclusion](/glossary#permanent-exclusion) rate in secondary schools increased 0.20% - up from 0.17% in 2015/16 - which is equivalent to 20 pupils per 10,000.

The rate also rose in primary schools to 0.03% but decreased in special schools to 0.07% - down from 0.08% in 2015/16.

The rate generally followed a downward trend after 2006/07 - when it stood at 0.12%.

However, since 2012/13 it has been on the rise although rates are still lower now than in 2006/07.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("8a8add13-368c-4067-9210-166bb19a00c1"),
                column: "MarkDownBlock_Body",
                value: @"The [persistent absence](/glossary#persistent-absence) rate increased to and accounted for 37.6% of all absence - up from 36.6% in 2015/16 but still down from 43.3% in 2011/12.

It also accounted for almost a third (31.6%) of all [authorised absence](/glossary#authorised-absence) and more than half (53.8%) of all [unauthorised absence](/glossary#unauthorised-absence).

Overall, it's increased across primary and secondary schools to 10.8% - up from 10.5% in 2015/16.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("97c54e5f-2406-4333-851d-b6c9cc4bf612"),
                column: "MarkDownBlock_Body",
                value: @"The [overall absence](/glossary#overall-absence) rate decreased to 5.1% - down from 5.2% for the previous two years.

Absence recorded for 4-year-olds is not treated as authorised or unauthorised and only reported as overall absence.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("97d414f4-1a27-4ed7-85ea-c4c903e1d8cb"),
                column: "MarkDownBlock_Body",
                value: @"The statistics and data cover permanent and fixed period exclusions and school-level exclusions during the 2016/17 academic year in the following state-funded school types as reported in the school census:

* primary schools

* secondary schools

* special schools

They also include national-level information on permanent and fixed-period exclusions for [pupil referral units](/glossary#pupil-referral-unit).

All figures are based on unrounded data so constituent parts may not add up due to rounding.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("99d75d39-7ea5-456e-979d-1215fa673a83"),
                column: "MarkDownBlock_Body",
                value: @"All reasons (except bullying and theft) saw an increase in [permanent exclusions](/glossary#permanent-exclusion) since 2015/16.

The following most common reasons saw the largest increases:

* physical assault against a pupil

* persistent disruptive behaviour

* other reasons

**Persistent disruptive behaviour**

Remained the most common reason for permanent exclusions accounting for 2,755 (35.7%) of all permanent exclusions - which is equivalent to 3 permanent exclusions per 10,000 pupils.

However, in special schools the most common reason for exclusion was physical assault against an adult - accounting for 37.8% of all permanent exclusions and 28.1% of all [fixed-period exclusions](/glossary#fixed-period-exclusion).

Persistent disruptive behaviour is also the most common reason for fixed-period exclusions accounting for 108,640 of all fixed-period exclusions - up from 27.7% in 2015/16. This is equivalent to around 135 fixed-period exclusions per 10,000 pupils.

All reasons saw an increase in fixed-period exclusions since 2015/16. Persistent disruptive behaviour and other reasons saw the largest increases.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("a0b85d7d-a9bd-48b5-82c6-a119adc74ca2"),
                column: "MarkDownBlock_Body",
                value: @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Pupil absence statistics: methodology](/methodology/pupil-absence-in-schools-in-england).

This release was created as example content during the platform’s Private Beta phase, whilst it provides access to real data, the below release should be used with some caution. To access the original, release please see [Pupil absence in schools in England: 2016 to 2017](https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2016-to-2017)");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("c73382ce-73ff-465f-8f1b-7a08cb6af089"),
                column: "MarkDownBlock_Body",
                value: @"There was a similar pattern to previous years where the following groups (where higher exclusion rates are expected) showed an increase in exclusions since 2015/16:

* boys

* national curriculum years 9 and 10

* pupils with special educational needs (SEN)

* pupils known to be eligible for and claiming free school meals (FSM)

**Age, national curriculum year group and gender**

* more than half of all [permanent exclusions](/glossary#permanent-exclusion) (57.2%) and [fixed-period exclusions](/glossary#fixed-period-exclusion) (52.6 %) occur in national curriculum year 9 or above

* a quarter (25%) of all permanent exclusions were for pupils aged 14 - who also had the highest rates for fixed-period exclusion and pupils receiving [one or more fixed-period exclusion](/glossary#one-or-more-fixed-period-exclusion)

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

* pupils of Asian ethnic groups had the lowest permanent and fixed-period exclusion rates");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("cf01208f-cbab-41d1-9fa5-4793d2a0bc13"),
                column: "MarkDownBlock_Body",
                value: @"Nearly half of all pupils (48.9%) were absent for 5 days or less across primary, secondary and special schools - down from 49.1% in 2015/16.

The average total absence for primary school pupils was 7.2 days compared to 16.9 days for special school and 9.3 day for secondary school pupils.

The rate of pupils who had more than 25 days of absence stayed the same as in 2015/16 at 4.3%.

These pupils accounted for 23.5% of days missed while 8.2% of pupils had no absence.

**Absence by term**

Across all schools:

* [overall absence](/glossary#overall-absence) - highest in summer and lowest in autumn

* [authorised absence](/glossary#authorised-absence) - highest in spring and lowest in summer

* [unauthorised absence](/glossary#unauthorised-absence) - highest in summer");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("d3288340-2689-4346-91a6-c070e7b0799d"),
                column: "MarkDownBlock_Body",
                value: @"**Permanent exclusion**

The [permanent exclusion](/glossary#permanent-exclusion) rate in [pupil referral units](/glossary#pupil-referral-unit) decreased to 0.13 - down from 0.14% in 2015/16.

Permanent exclusions rates have remained fairly steady following an increase between 2013/14 and 2014/15.

**Fixed-period exclusion**

The [fixed period exclusion](/glossary#fixed-period-exclusion) rate has been steadily increasing since 2013/14.

The percentage of pupils in pupil referral units who 1 or more fixed-period exclusion increased to 59.17% - up from 58.15% in 2015/16.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("d988a5e8-4e3c-4c1d-b5a9-bf0e1d947085"),
                column: "MarkDownBlock_Body",
                value: "There were 560 reviews lodged with [independent review panels](/glossary#independent-review-panel) in maintained primary, secondary and special schools and academies of which 525 (93.4%) were determined and 45 (8.0%) resulted in an offer of reinstatement.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("eb4318f9-11e0-46ea-9796-c36a9dc25014"),
                column: "MarkDownBlock_Body",
                value: @"The [overall absence](/glossary#overall-absence) and [persistent absence](/glossary#persistent-absence) patterns for pupils with different characteristics have been consistent over recent years.
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

* pupils with a SEN statement or education healthcare (EHC) plan - more than 2 times higher than pupils with no identified SEN");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("13e4577a-2291-4ce4-a8c9-6c76baa06092"),
                column: "MarkDownBlock_Body",
                value: @"**Secondary applications**

The number of applications received for secondary school places increased to 582,761 - up 3.6% since 2017. This follows a 2.6% increase between 2016 and 2017.

This continues the increase in secondary applications seen since 2013 which came on the back of a rise in births which began in the previous decade.

Since 2013, when secondary applications were at their lowest, there has been a 16.6% increase in the number of applications.

**Secondary offers**

The proportion of secondary applicants receiving an offer of their first-choice school has decreased to 82.1% - down from 83.5% in 2017.

The proportion of applicants who received an offer of any of their preferred schools also decreased slightly to 95.5% - down from 96.1% in 2017.

**Secondary National Offer Day**

These statistics come from the process undertaken by local authorities (LAs) which enabled them to send out offers of secondary school places to all applicants on the [Secondary National Offer Day](../glossary#national-offer-day) of 1 March 2018.

The secondary figures have been collected since 2008 and can be viewed as a time series in the following table.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("1a1d29f6-c4d5-41a9-9a06-b2ce84043edd"),
                column: "MarkDownBlock_Body",
                value: @"There's considerable variation in the [permanent exclusion](../glossary#permanent-exclusion) and [fixed-period exclusion](../glossary#fixed-period-exclusion) rate at the LA level.

**Permanent exclusion**

Similar to 2015/16, the regions with the joint-highest rates across all school types were:

* North West - 0.14%

* North West - 0.14%

Similar to 2015/16, the regions with the lowest rates were:

* South East - 0.06%

* Yorkshire and the Humber - 0.07%

**Fixed-period exclusion**

Similar to 2015/16, the region with the highest rates across all school types was Yorkshire and the Humber at 7.22% while the lowest rate was in Outer London (3.49%).");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("2c369594-3bbc-40b4-ad19-196c923f5c7f"),
                column: "MarkDownBlock_Body",
                value: @"**Overall absence**

The [overall absence](../glossary#overall-absence) rate has increased across state-funded primary, secondary and special schools between 2015/16 and 2016/17 driven by an increase in the unauthorised absence rate.

It increased from 4.6% to 4.7% over this period while the [unauthorised absence](../glossary#unauthorised-absence) rate increased from 1.1% to 1.3%.

The rate stayed the same at 4% in primary schools but increased from 5.2% to 5.4% for secondary schools. However, in special schools it was much higher and rose to 9.7%.

The overall and [authorised absence](../glossary#authorised-absence) rates have been fairly stable over recent years after gradually decreasing between 2006/07 and 2013/14.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("2ef5f84f-e151-425d-8906-2921712f9157"),
                column: "MarkDownBlock_Body",
                value: @"**Illness**

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

They also partially relate to the period after the April 2017 Supreme Court judgment where it unanimously agreed that no children should be taken out of school without good reason and clarified that 'regularly' means 'in accordance with the rules prescribed by the school'.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("31c6b325-cbfa-4108-9956-cde7fa6a99ec"),
                column: "MarkDownBlock_Body",
                value: @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Secondary and primary school applications and offers: methodology](../methodology/secondary-and-primary-schools-applications-and-offers)");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("3913a0af-9455-4802-a037-c4cfd4719b18"),
                column: "MarkDownBlock_Body",
                value: @"**Unauthorised absence**

The [unauthorised absence](../glossary#unauthorised-absence) rate has not varied much since 2006/07 but is at its highest since records began - 1.3%.

This is due to an increase in absence due to family holidays not agreed by schools.

**Authorised absence**

The [authorised absence](../glossary#authorised-absence) rate has stayed at 3.4% since 2015/16 but has been decreasing in recent years within primary schools.

**Total number of days missed**

The total number of days missed for [overall absence](../glossary#overall-absence) across state-funded primary, secondary and special schools has increased to 56.7 million from 54.8 million in 2015/16.

This partly reflects a rise in the total number of pupils with the average number of days missed per pupil slightly increased to 8.2 days from 8.1 days in 2015/16.

In 2016/17, 91.8% of primary, secondary and special school pupils missed at least 1 session during the school year - similar to the 91.7% figure from 2015/16.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("3aaafa20-bc32-4523-bb23-dd55c458f928"),
                column: "MarkDownBlock_Body",
                value: @"The [overall absence](../glossary#overall-absence) rate increased to 33.9% - up from 32.6% in 2015/16.

The [persistent absence](../glossary#persistent-absence) rate increased to 73.9% - up from 72.5% in 2015/16.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4aa06200-406b-4f5a-bee4-19e3b83eb1d2"),
                column: "MarkDownBlock_Body",
                value: @"**Persistent absentees**

The [overall absence](../glossary#overall-absence) rate for persistent absentees across all schools increased to 18.1% - nearly 4 times higher than the rate for all pupils. This is slightly up from 17.6% in 2015/16.

**Illness absence rate**

The illness absence rate is almost 4 times higher for persistent absentees at 7.6% compared to 2% for other pupils.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4e05bbb3-bd4e-4602-8424-069e59034c87"),
                column: "MarkDownBlock_Body",
                value: @"**Pupils with one or more fixed-period exclusion definition**

The number of pupils with [one or more fixed-period exclusion](../glossary#one-or-more-fixed-period-exclusion) has increased across state-funded primary, secondary and special schools to 183,475 (2.29% of pupils) up from 167,125 (2.11% of pupils) in 2015/16.

Of these kinds of pupils, 59.1% excluded on only 1 occasion while 1.5% received 10 or more fixed-period exclusions during the year.

The percentage of pupils who went on to receive a [permanent exclusion](../glossary#permanent-exclusion) was 3.5%.

The average length of [fixed-period exclusion](../glossary#fixed-period-exclusion) across schools decreased to 2.1 days - slightly shorter than in 2015/16.

The highest proportion of fixed-period exclusions (46.6%) lasted for only 1 day.

Only 2.0% of fixed-period exclusions lasted for longer than 1 week and longer fixed-period exclusions were more prevalent in secondary schools.");

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
                keyValue: new Guid("7934e93d-2e11-478d-ab0e-f799f15164bb"),
                column: "MarkDownBlock_Body",
                value: @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Permanent and fixed-period exclusion statistics: methodology](../methodology/permanent-and-fixed-period-exclusions-in-england)

This release was created as example content during the platform’s Private Beta phase, whilst it provides access to real data, the below release should be used with some caution. To access the original release, please see [Permanent and fixed-period exclusions in England: 2016 to 2017](https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2016-to-2017)");

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
                keyValue: new Guid("97d414f4-1a27-4ed7-85ea-c4c903e1d8cb"),
                column: "MarkDownBlock_Body",
                value: @"The statistics and data cover permanent and fixed period exclusions and school-level exclusions during the 2016/17 academic year in the following state-funded school types as reported in the school census:

* primary schools

* secondary schools

* special schools

They also include national-level information on permanent and fixed-period exclusions for [pupil referral units](../glossary#pupil-referral-unit).

All figures are based on unrounded data so constituent parts may not add up due to rounding.");

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
                keyValue: new Guid("a0b85d7d-a9bd-48b5-82c6-a119adc74ca2"),
                column: "MarkDownBlock_Body",
                value: @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Pupil absence statistics: methodology](../methodology/pupil-absence-in-schools-in-england).

This release was created as example content during the platform’s Private Beta phase, whilst it provides access to real data, the below release should be used with some caution. To access the original, release please see [Pupil absence in schools in England: 2016 to 2017](https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2016-to-2017)");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("c73382ce-73ff-465f-8f1b-7a08cb6af089"),
                column: "MarkDownBlock_Body",
                value: @"There was a similar pattern to previous years where the following groups (where higher exclusion rates are expected) showed an increase in exclusions since 2015/16:

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

* pupils of Asian ethnic groups had the lowest permanent and fixed-period exclusion rates");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("cf01208f-cbab-41d1-9fa5-4793d2a0bc13"),
                column: "MarkDownBlock_Body",
                value: @"Nearly half of all pupils (48.9%) were absent for 5 days or less across primary, secondary and special schools - down from 49.1% in 2015/16.

The average total absence for primary school pupils was 7.2 days compared to 16.9 days for special school and 9.3 day for secondary school pupils.

The rate of pupils who had more than 25 days of absence stayed the same as in 2015/16 at 4.3%.

These pupils accounted for 23.5% of days missed while 8.2% of pupils had no absence.

**Absence by term**

Across all schools:

* [overall absence](../glossary#overall-absence) - highest in summer and lowest in autumn

* [authorised absence](../glossary#authorised-absence) - highest in spring and lowest in summer

* [unauthorised absence](../glossary#unauthorised-absence) - highest in summer");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("d3288340-2689-4346-91a6-c070e7b0799d"),
                column: "MarkDownBlock_Body",
                value: @"**Permanent exclusion**

The [permanent exclusion](../glossary#permanent-exclusion) rate in [pupil referral units](../glossary#pupil-referral-unit) decreased to 0.13 - down from 0.14% in 2015/16.

Permanent exclusions rates have remained fairly steady following an increase between 2013/14 and 2014/15.

**Fixed-period exclusion**

The [fixed period exclusion](../glossary#fixed-period-exclusion) rate has been steadily increasing since 2013/14.

The percentage of pupils in pupil referral units who 1 or more fixed-period exclusion increased to 59.17% - up from 58.15% in 2015/16.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("d988a5e8-4e3c-4c1d-b5a9-bf0e1d947085"),
                column: "MarkDownBlock_Body",
                value: "There were 560 reviews lodged with [independent review panels](../glossary#independent-review-panel) in maintained primary, secondary and special schools and academies of which 525 (93.4%) were determined and 45 (8.0%) resulted in an offer of reinstatement.");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("eb4318f9-11e0-46ea-9796-c36a9dc25014"),
                column: "MarkDownBlock_Body",
                value: @"The [overall absence](../glossary#overall-absence) and [persistent absence](../glossary#persistent-absence) patterns for pupils with different characteristics have been consistent over recent years.
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

* pupils with a SEN statement or education healthcare (EHC) plan - more than 2 times higher than pupils with no identified SEN");
        }
    }
}
