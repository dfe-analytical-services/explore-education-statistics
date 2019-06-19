using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class UpdateSummaryBlocksToAddTablesAndCharts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                columns: new[] { "KeyStatistics", "Summary" },
                values: new object[] { "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":1,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2012\",\"endYear\":\"2016\",\"filters\":[\"1\",\"2\"],\"indicators\":[\"23\",\"26\",\"28\"]},\"Charts\":[{\"Indicators\":[\"23\",\"26\",\"28\"],\"XAxis\":{\"title\":\"School Year\"},\"YAxis\":{\"title\":\"Absence Rate\"},\"Type\":\"line\"}],\"Summary\":{\"dataKeys\":[\"26\",\"28\",\"23\"],\"dataSummary\":[\"Up from 4.6% in 2015/16\",\"Similar to previous years\",\"Up from 1.1% in 2015/16\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * pupils missed on average 8.2 school days\\n * overall and unauthorised absence rates up on 2015/16\\n * unauthorised absence rise due to higher rates of unauthorised holidays\\n * 10% of pupils persistently absent during 2016/17\"}},\"Tables\":[{\"indicators\":[\"23\",\"26\",\"28\"]}]}", @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Pupil absence statistics: methodology](../methodology/pupil-absence-in-schools-in-england)." });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                columns: new[] { "KeyStatistics", "Summary" },
                values: new object[] { "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":17,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2014\",\"endYear\":\"2018\",\"filters\":[\"845\"],\"indicators\":[\"189\",\"193\",\"194\",\"195\",\"196\",\"197\",\"198\",\"199\"]},\"Charts\":null,\"Summary\":{\"dataKeys\":[\"189\",\"193\",\"194\"],\"dataSummary\":[\"Down from 620,330 in 2017\",\"Down from 558,411 in 2017\",\"Down from 34,792 in 2017\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\"* majority of applicants received a preferred offer\\n* percentage of applicants receiving secondary first choice offers decreases as applications increase\\n* slight proportional increase in applicants receiving primary first choice offer as applications decrease\\n\"}},\"Tables\":[{\"indicators\":[\"189\",\"193\",\"194\",\"195\",\"198\",\"199\"]}]}", @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Secondary and primary school applications and offers: methodology](../methodology/secondary-and-primary-schools-applications-and-offers)" });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                columns: new[] { "KeyStatistics", "Summary" },
                values: new object[] { "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":12,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2012\",\"endYear\":\"2016\",\"filters\":[\"727\"],\"indicators\":[\"153\",\"154\",\"155\",\"156\",\"157\",\"158\",\"160\"]},\"Charts\":[{\"Indicators\":[\"158\",\"160\"],\"XAxis\":{\"title\":\"School Year\"},\"YAxis\":{\"title\":\"\"},\"Type\":\"line\"}],\"Summary\":{\"dataKeys\":[\"156\",\"158\",\"155\"],\"dataSummary\":[\"Up from 0.08 in 2015/16\",\"Up from 4.29% in 2015/16\",\"Up from 6,685 in 2015/16\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * overall permanent exclusions rate has increased to 0.10% - up from 0.08% in 2015/16\\n * number of exclusions increased to 7,720 - up from 6,685 in 2015/16\\n * overall fixed-period exclusions rate increased to 4.76% - up from 4.29% in 2015/16\\n * number of exclusions increased to 381,865 - up from 339,360 in 2015/16\\n\"}},\"Tables\":[{\"indicators\":[\"156\",\"158\",\"155\"]}]}", @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Permanent and fixed-period exclusion statistics: methodology](../methodology/permanent-and-fixed-period-exclusions-in-england)" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                columns: new[] { "KeyStatistics", "Summary" },
                values: new object[] { "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":1,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2012\",\"endYear\":\"2016\",\"filters\":[\"1\",\"2\"],\"indicators\":[\"23\",\"26\",\"28\"]},\"Charts\":null,\"Summary\":{\"dataKeys\":[\"26\",\"28\",\"23\"],\"dataSummary\":[\"Up from 4.6% in 2015/16\",\"Similar to previous years\",\"Up from 1.1% in 2015/16\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * pupils missed on average 8.2 school days\\n * overall and unauthorised absence rates up on 2015/16\\n * unauthorised absence rise due to higher rates of unauthorised holidays\\n * 10% of pupils persistently absent during 2016/17\"}},\"Tables\":null}", @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Pupil absence statistics: methodology](../methodology/pupil-absence-in-schools-in-england)." });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                columns: new[] { "KeyStatistics", "Summary" },
                values: new object[] { "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":17,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2018\",\"endYear\":\"2018\",\"filters\":[\"845\"],\"indicators\":[\"189\",\"193\",\"194\"]},\"Charts\":null,\"Summary\":{\"dataKeys\":[\"189\",\"193\",\"194\"],\"dataSummary\":[\"Down from 620,330 in 2017\",\"Down from 558,411 in 2017\",\"Down from 34,792 in 2017\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\"* majority of applicants received a preferred offer\\n* percentage of applicants receiving secondary first choice offers decreases as applications increase\\n* slight proportional increase in applicants receiving primary first choice offer as applications decrease\\n\"}},\"Tables\":null}", @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Secondary and primary school applications and offers: methodology](../methodology/secondary-and-primary-schools-applications-and-offers)" });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                columns: new[] { "KeyStatistics", "Summary" },
                values: new object[] { "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":12,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2012\",\"endYear\":\"2016\",\"filters\":[\"727\"],\"indicators\":[\"153\",\"154\",\"155\",\"156\",\"158\"]},\"Charts\":null,\"Summary\":{\"dataKeys\":[\"156\",\"158\",\"155\"],\"dataSummary\":[\"Up from 0.08 in 2015/16\",\"Up from 4.29% in 2015/16\",\"Up from 6,685 in 2015/16\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * overall permanent exclusions rate has increased to 0.10% - up from 0.08% in 2015/16\\n * number of exclusions increased to 7,720 - up from 6,685 in 2015/16\\n * overall fixed-period exclusions rate increased to 4.76% - up from 4.29% in 2015/16\\n * number of exclusions increased to 381,865 - up from 339,360 in 2015/16\\n\"}},\"Tables\":null}", @"Read national statistical summaries, view charts and tables and download data files.

Find out how and why these statistics are collected and published - [Permanent and fixed-period exclusion statistics: methodology](../methodology/permanent-and-fixed-period-exclusions-in-england)" });
        }
    }
}
