using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class AddContactAndDataSummaries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                table: "Publications",
                nullable: true,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TeamName = table.Column<string>(nullable: true),
                    TeamEmail = table.Column<string>(nullable: true),
                    ContactName = table.Column<string>(nullable: true),
                    ContactTelNo = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Contacts",
                columns: new[] { "Id", "ContactName", "ContactTelNo", "TeamEmail", "TeamName" },
                values: new object[] { new Guid("11bb7387-e85e-4571-9669-8a760dcb004f"), "Simon Shakespeare", "0114 262 1619", "teamshakes@gmail.com", "Simon's Team" });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":1,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2012\",\"endYear\":\"2016\",\"filters\":[\"1\",\"2\"],\"indicators\":[\"23\",\"26\",\"28\"]},\"Charts\":null,\"Summary\":{\"dataKeys\":[\"23\",\"26\",\"28\"],\"dataSummary\":[\"Up from 40.1 in 2015/16\",\"Down from 40.1 in 2015/16\",\"Up from 40.1 in 2015/16\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * pupils missed on average 8.2 school days\\n * overall and unauthorised absence rates up on 2015/16\\n * unauthorised absence rise due to higher rates of unauthorised holidays\\n * 10% of pupils persistently absent during 2016/17\"}},\"Tables\":null}");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":17,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2014\",\"endYear\":\"2018\",\"filters\":[\"845\"],\"indicators\":[\"193\"]},\"Charts\":null,\"Summary\":{\"dataKeys\":[\"193\"],\"dataSummary\":null,\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\"* majority of applicants received a preferred offer\\n* percentage of applicants receiving secondary first choice offers decreases as applications increase\\n* slight proportional increase in applicants receiving primary first choice offer as applications decrease\\n\"}},\"Tables\":null}");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":12,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2012\",\"endYear\":\"2016\",\"filters\":[\"727\"],\"indicators\":[\"153\",\"154\",\"155\",\"156\",\"158\"]},\"Charts\":null,\"Summary\":{\"dataKeys\":[\"155\",\"156\",\"158\"],\"dataSummary\":[\"Up from 40.1 in 2015/16\",\"Down from 40.1 in 2015/16\",\"Up from 40.1 in 2015/16\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * overall permanent exclusions rate has increased to 0.10% - up from 0.08% in 2015/16\\n * number of exclusions increased to 7,720 - up from 6,685 in 2015/16\\n * overall fixed-period exclusions rate increased to 4.76% - up from 4.29% in 2015/16\\n * number of exclusions increased to 381,865 - up from 339,360 in 2015/16\\n\"}},\"Tables\":null}");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"--\",\"--\",\"--\"],\"dataSummary\":[\"\",\"\",\"\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\"\"}},\"Tables\":null}");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                column: "ContactId",
                value: new Guid("11bb7387-e85e-4571-9669-8a760dcb004f"));

            migrationBuilder.CreateIndex(
                name: "IX_Publications_ContactId",
                table: "Publications",
                column: "ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_Publications_Contacts_ContactId",
                table: "Publications",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Publications_Contacts_ContactId",
                table: "Publications");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Publications_ContactId",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "Publications");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":1,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2012\",\"endYear\":\"2016\",\"filters\":[\"1\",\"2\"],\"indicators\":[\"23\",\"26\",\"28\"]},\"Charts\":null,\"Summary\":{\"dataKeys\":[\"23\",\"26\",\"28\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * pupils missed on average 8.2 school days\\n * overall and unauthorised absence rates up on 2015/16\\n * unauthorised absence rise due to higher rates of unauthorised holidays\\n * 10% of pupils persistently absent during 2016/17\"}},\"Tables\":null}");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":17,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2014\",\"endYear\":\"2018\",\"filters\":[\"845\"],\"indicators\":[\"193\"]},\"Charts\":null,\"Summary\":{\"dataKeys\":[\"193\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\"* majority of applicants received a preferred offer\\n* percentage of applicants receiving secondary first choice offers decreases as applications increase\\n* slight proportional increase in applicants receiving primary first choice offer as applications decrease\\n\"}},\"Tables\":null}");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":12,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2012\",\"endYear\":\"2016\",\"filters\":[\"727\"],\"indicators\":[\"153\",\"154\",\"155\",\"156\",\"158\"]},\"Charts\":null,\"Summary\":{\"dataKeys\":[\"155\",\"156\",\"158\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * overall permanent exclusions rate has increased to 0.10% - up from 0.08% in 2015/16\\n * number of exclusions increased to 7,720 - up from 6,685 in 2015/16\\n * overall fixed-period exclusions rate increased to 4.76% - up from 4.29% in 2015/16\\n * number of exclusions increased to 381,865 - up from 339,360 in 2015/16\\n\"}},\"Tables\":null}");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"--\",\"--\",\"--\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\"\"}},\"Tables\":null}");
        }
    }
}
