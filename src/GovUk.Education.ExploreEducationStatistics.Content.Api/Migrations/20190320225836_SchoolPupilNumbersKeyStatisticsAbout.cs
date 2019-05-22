using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class SchoolPupilNumbersKeyStatisticsAbout : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"),
                column: "Content",
                value: "[{\"Order\":1,\"Heading\":\"About this release\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"This statistical publication provides the number of schools and pupils in schools in England, using data from the January 2018School Census.\\n\\n Breakdowns are given for school types as well as for pupil characteristics including free school meal eligibility, English as an additional languageand ethnicity.This release also contains information about average class sizes.\\n\\n SEN tables previously provided in thispublication will be published in the statistical publication ‘Special educational needs in England: January 2018’ scheduled for release on 26July 2018.\\n\\n Cross border movement tables will be added to this publication later this year.\"}]}]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"),
                column: "Content",
                value: "[{\"Order\":1,\"Heading\":\"About this release\",\"Caption\":\"\",\"Content\":null}]");
        }
    }
}
