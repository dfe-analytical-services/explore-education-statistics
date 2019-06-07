using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class AddDataSummary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":1,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2016\",\"endYear\":\"2017\",\"filters\":[\"1\",\"2\"],\"indicators\":[\"23\",\"26\",\"28\"]},\"Charts\":null,\"Summary\":{\"dataKeys\":[\"23\",\"26\",\"28\"],\"dataSummary\":[\"Up from 40.1 in 2015/16\",\"Down from 40.1 in 2015/16\",\"Up from 40.1 in 2015/16\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * pupils missed on average 8.2 school days \\n  * overall and unauthorised absence rates up on previous year \\n * unauthorised rise due to higher rates of unauthorised holidays \\n * 10% of pupils persistently absent during 2016/17\"}},\"Tables\":null}");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":\"Latest headline facts and figures - 2016 to 2017\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"--\",\"--\",\"--\"],\"dataSummary\":[\"\",\"\",\"\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\"\"}},\"Tables\":null}");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"perm_excl_rate\",\"perm_excl\",\"fixed_excl_rate\"],\"dataSummary\":[\"Up from 40.1 in 2015/16\",\"Down from 40.1 in 2015/16\",\"Up from 40.1 in 2015/16\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * overall rate of permanent exclusions has increased from 0.08 per cent of pupil enrolments in 2015/16 to 0.10 per cent in 2016/17 \\n * number of exclusions has also increased, from 6,685 to 7,720 \\n * overall rate of fixed period exclusions increased, from 4.29 per cent of pupil enrolments in 2015/16 to 4.76 per cent in 2016/17 \\n * number of exclusions has also increased, from 339,360 to 381,865. \\n\"}},\"Tables\":null}");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7ae88fb-afaf-4d51-a78a-bbb2de671daf"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":\"Latest headline facts and figures - 2016 to 2017\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"--\",\"--\",\"--\"],\"dataSummary\":[\"\",\"\",\"\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * average Attainment8 scores remained stable compared to 2017s \\n * percentage of pupils achieving 5 or above in English and Maths increased \\n * EBacc entry increased slightly \\n * over 250 schools met the coasting definition in 2018\"}},\"Tables\":null}");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"--\",\"--\",\"--\"],\"dataSummary\":[\"\",\"\",\"\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\"\"}},\"Tables\":null}");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":1,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2016\",\"endYear\":\"2017\",\"filters\":[\"1\",\"2\"],\"indicators\":[\"23\",\"26\",\"28\"]},\"Charts\":null,\"Summary\":{\"dataKeys\":[\"23\",\"26\",\"28\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * pupils missed on average 8.2 school days \\n  * overall and unauthorised absence rates up on previous year \\n * unauthorised rise due to higher rates of unauthorised holidays \\n * 10% of pupils persistently absent during 2016/17\"}},\"Tables\":null}");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":\"Latest headline facts and figures - 2016 to 2017\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"--\",\"--\",\"--\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\"\"}},\"Tables\":null}");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"perm_excl_rate\",\"perm_excl\",\"fixed_excl_rate\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * overall rate of permanent exclusions has increased from 0.08 per cent of pupil enrolments in 2015/16 to 0.10 per cent in 2016/17 \\n * number of exclusions has also increased, from 6,685 to 7,720 \\n * overall rate of fixed period exclusions increased, from 4.29 per cent of pupil enrolments in 2015/16 to 4.76 per cent in 2016/17 \\n * number of exclusions has also increased, from 339,360 to 381,865. \\n\"}},\"Tables\":null}");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7ae88fb-afaf-4d51-a78a-bbb2de671daf"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":\"Latest headline facts and figures - 2016 to 2017\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"--\",\"--\",\"--\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * average Attainment8 scores remained stable compared to 2017s \\n * percentage of pupils achieving 5 or above in English and Maths increased \\n * EBacc entry increased slightly \\n * over 250 schools met the coasting definition in 2018\"}},\"Tables\":null}");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"--\",\"--\",\"--\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\"\"}},\"Tables\":null}");
        }
    }
}
