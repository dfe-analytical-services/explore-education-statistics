using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class NationalStatisticsReleaseType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ReleaseTypes",
                columns: new[] { "Id", "Title" },
                values: new object[] { new Guid("8becd272-1100-4e33-8a7d-1c0c4e3b42b8"), "National Statistics" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ReleaseTypes",
                keyColumn: "Id",
                keyValue: new Guid("8becd272-1100-4e33-8a7d-1c0c4e3b42b8"));
        }
    }
}
