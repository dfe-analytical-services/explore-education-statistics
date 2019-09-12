using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class SimonContactRemoved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("11bb7387-e85e-4571-9669-8a760dcb004f"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Contacts",
                columns: new[] { "Id", "ContactName", "ContactTelNo", "TeamEmail", "TeamName" },
                values: new object[] { new Guid("11bb7387-e85e-4571-9669-8a760dcb004f"), "Simon Shakespeare", "0114 262 1619", "teamshakes@gmail.com", "Simon's Team" });
        }
    }
}
