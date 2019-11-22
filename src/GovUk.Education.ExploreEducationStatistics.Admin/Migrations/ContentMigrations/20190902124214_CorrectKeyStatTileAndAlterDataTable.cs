using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    [ExcludeFromCodeCoverage]
    public partial class CorrectKeyStatTileAndAlterDataTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Id\":\"475738b4-ba10-4c29-a50d-6ca82c10de6e\",\"Heading\":null,\"DataBlockRequest\":{\"SubjectId\":17,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"571\"],\"Indicators\":[\"211\",\"212\",\"216\",\"217\",\"218\",\"219\",\"220\",\"221\",\"222\"],\"Country\":null,\"LocalAuthority\":null,\"Region\":null},\"Charts\":null,\"Summary\":{\"dataKeys\":[\"212\",\"216\",\"217\"],\"dataSummary\":[\"Down from 620,330 in 2017\",\"Down from 558,411 in 2017\",\"Down from 34,792 in 2017\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\"* majority of applicants received a preferred offer\\n* percentage of applicants receiving secondary first choice offers decreases as applications increase\\n* slight proportional increase in applicants receiving primary first choice offer as applications decrease\\n\"}},\"Tables\":[{\"indicators\":[\"212\",\"211\",\"216\",\"217\",\"218\",\"221\",\"222\"]}]}");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Id\":\"475738b4-ba10-4c29-a50d-6ca82c10de6e\",\"Heading\":null,\"DataBlockRequest\":{\"SubjectId\":17,\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"571\"],\"Indicators\":[\"212\",\"215\",\"217\",\"218\",\"219\",\"220\",\"221\",\"222\"],\"Country\":null,\"LocalAuthority\":null,\"Region\":null},\"Charts\":null,\"Summary\":{\"dataKeys\":[\"212\",\"215\",\"217\"],\"dataSummary\":[\"Down from 620,330 in 2017\",\"Down from 558,411 in 2017\",\"Down from 34,792 in 2017\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\"* majority of applicants received a preferred offer\\n* percentage of applicants receiving secondary first choice offers decreases as applications increase\\n* slight proportional increase in applicants receiving primary first choice offer as applications decrease\\n\"}},\"Tables\":[{\"indicators\":[\"212\",\"215\",\"217\",\"218\",\"221\",\"222\"]}]}");
        }
    }
}
