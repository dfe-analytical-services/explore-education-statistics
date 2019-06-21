using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class UpdatingMinorContentChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":12,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2012\",\"endYear\":\"2016\",\"filters\":[\"727\"],\"indicators\":[\"153\",\"154\",\"155\",\"156\",\"157\",\"158\",\"160\"]},\"Charts\":[{\"Indicators\":[\"158\",\"160\"],\"XAxis\":{\"title\":\"School Year\"},\"YAxis\":{\"title\":\"\"},\"Type\":\"line\"}],\"Summary\":{\"dataKeys\":[\"156\",\"158\",\"155\"],\"dataSummary\":[\"Up from 0.08% in 2015/16\",\"Up from 4.29% in 2015/16\",\"Up from 6,685 in 2015/16\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * overall permanent exclusions rate has increased to 0.10% - up from 0.08% in 2015/16\\n * number of exclusions increased to 7,720 - up from 6,685 in 2015/16\\n * overall fixed-period exclusions rate increased to 4.76% - up from 4.29% in 2015/16\\n * number of exclusions increased to 381,865 - up from 339,360 in 2015/16\\n\"}},\"Tables\":[{\"indicators\":[\"156\",\"158\",\"155\"]}]}");

            migrationBuilder.UpdateData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("18e0d40e-bdf7-4c84-99dd-732e72e9c9a5"),
                column: "On",
                value: new DateTime(2018, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("9c0f0139-7f88-4750-afe0-1c85cdf1d047"),
                column: "On",
                value: new DateTime(2018, 4, 19, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":12,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2012\",\"endYear\":\"2016\",\"filters\":[\"727\"],\"indicators\":[\"153\",\"154\",\"155\",\"156\",\"157\",\"158\",\"160\"]},\"Charts\":[{\"Indicators\":[\"158\",\"160\"],\"XAxis\":{\"title\":\"School Year\"},\"YAxis\":{\"title\":\"\"},\"Type\":\"line\"}],\"Summary\":{\"dataKeys\":[\"156\",\"158\",\"155\"],\"dataSummary\":[\"Up from 0.08 in 2015/16\",\"Up from 4.29% in 2015/16\",\"Up from 6,685 in 2015/16\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * overall permanent exclusions rate has increased to 0.10% - up from 0.08% in 2015/16\\n * number of exclusions increased to 7,720 - up from 6,685 in 2015/16\\n * overall fixed-period exclusions rate increased to 4.76% - up from 4.29% in 2015/16\\n * number of exclusions increased to 381,865 - up from 339,360 in 2015/16\\n\"}},\"Tables\":[{\"indicators\":[\"156\",\"158\",\"155\"]}]}");

            migrationBuilder.UpdateData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("18e0d40e-bdf7-4c84-99dd-732e72e9c9a5"),
                column: "On",
                value: new DateTime(2017, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("9c0f0139-7f88-4750-afe0-1c85cdf1d047"),
                column: "On",
                value: new DateTime(2017, 4, 19, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
