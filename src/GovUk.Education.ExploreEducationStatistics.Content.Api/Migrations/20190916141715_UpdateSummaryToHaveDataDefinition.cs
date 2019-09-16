using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class UpdateSummaryToHaveDataDefinition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"179\",\"181\",\"178\"],\"dataSummary\":[\"Up from 0.08% in 2015/16\",\"Up from 4.29% in 2015/16\",\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Number of permanent exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\",\"Number of fixed-period exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\",\"Total number of permanent exclusions within a school year. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"description\":{\"Body\":\" * overall permanent exclusions rate has increased to 0.10% - up from 0.08% in 2015/16\\n * number of exclusions increased to 7,720 - up from 6,685 in 2015/16\\n * overall fixed-period exclusions rate increased to 4.76% - up from 4.29% in 2015/16\\n * number of exclusions increased to 381,865 - up from 339,360 in 2015/16\\n\",\"Type\":\"MarkDownBlock\",\"Id\":\"132bef6e-c2a3-459d-996e-40f29ed6e74f\",\"ContentSection\":null,\"ContentSectionId\":null}}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"212\",\"216\",\"217\"],\"dataSummary\":[\"Down from 620,330 in 2017\",\"Down from 558,411 in 2017\",\"Down from 34,792 in 2017\"],\"dataDefinition\":[\"Total number of applications received for places at primary and secondary schools.\",\"Total number of first preferences offered to applicants by schools.\",\"Total number of second preferences offered to applicants by schools.\"],\"description\":{\"Body\":\"* majority of applicants received a preferred offer\\n* percentage of applicants receiving secondary first choice offers decreases as applications increase\\n* slight proportional increase in applicants receiving primary first choice offer as applications decrease\\n\",\"Type\":\"MarkDownBlock\",\"Id\":\"fdcac9d3-dab5-445d-9802-a8af0990efb2\",\"ContentSection\":null,\"ContentSectionId\":null}}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"26\",\"28\",\"23\"],\"dataSummary\":[\"Up from 4.6% in 2015/16\",\"Similar to previous years\",\"Up from 1.1% in 2015/16\"],\"dataDefinition\":[\"Total number of all authorised and unauthorised absences from possible school sessions for all pupils. <a href=\\\"/glossary#overall-absence\\\">More >>></a>\",\"Number of authorised absences as a percentage of the overall school population. <a href=\\\"/glossary#authorised-absence\\\">More >>></a>\",\"Number of unauthorised absences as a percentage of the overall school population. <a href=\\\"/glossary#unauthorised-absence\\\">More >>></a>\"],\"description\":{\"Body\":\" * pupils missed on average 8.2 school days\\n * overall and unauthorised absence rates up on 2015/16\\n * unauthorised absence rise due to higher rates of unauthorised holidays\\n * 10% of pupils persistently absent during 2016/17\",\"Type\":\"MarkDownBlock\",\"Id\":\"f928762e-9bd5-4538-a4f0-d7f34b2874e6\",\"ContentSection\":null,\"ContentSectionId\":null}}");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"179\",\"181\",\"178\"],\"dataSummary\":[\"Up from 0.08% in 2015/16\",\"Up from 4.29% in 2015/16\",\"Up from 6,685 in 2015/16\"],\"description\":{\"Body\":\" * overall permanent exclusions rate has increased to 0.10% - up from 0.08% in 2015/16\\n * number of exclusions increased to 7,720 - up from 6,685 in 2015/16\\n * overall fixed-period exclusions rate increased to 4.76% - up from 4.29% in 2015/16\\n * number of exclusions increased to 381,865 - up from 339,360 in 2015/16\\n\",\"Type\":\"MarkDownBlock\",\"Id\":\"132bef6e-c2a3-459d-996e-40f29ed6e74f\",\"ContentSection\":null,\"ContentSectionId\":null}}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"212\",\"216\",\"217\"],\"dataSummary\":[\"Down from 620,330 in 2017\",\"Down from 558,411 in 2017\",\"Down from 34,792 in 2017\"],\"description\":{\"Body\":\"* majority of applicants received a preferred offer\\n* percentage of applicants receiving secondary first choice offers decreases as applications increase\\n* slight proportional increase in applicants receiving primary first choice offer as applications decrease\\n\",\"Type\":\"MarkDownBlock\",\"Id\":\"fdcac9d3-dab5-445d-9802-a8af0990efb2\",\"ContentSection\":null,\"ContentSectionId\":null}}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"26\",\"28\",\"23\"],\"dataSummary\":[\"Up from 4.6% in 2015/16\",\"Similar to previous years\",\"Up from 1.1% in 2015/16\"],\"description\":{\"Body\":\" * pupils missed on average 8.2 school days\\n * overall and unauthorised absence rates up on 2015/16\\n * unauthorised absence rise due to higher rates of unauthorised holidays\\n * 10% of pupils persistently absent during 2016/17\",\"Type\":\"MarkDownBlock\",\"Id\":\"f928762e-9bd5-4538-a4f0-d7f34b2874e6\",\"ContentSection\":null,\"ContentSectionId\":null}}");
        }
    }
}
