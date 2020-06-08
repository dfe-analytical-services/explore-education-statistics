using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES971ChangePublicationContact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("74f5aade-6d24-4a0b-be23-2ab4b4b2d191"),
                column: "ContactTelNo",
                value: "0370 000 2288");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("74f5aade-6d24-4a0b-be23-2ab4b4b2d191"),
                column: "ContactTelNo",
                value: "020 7783 8553");
        }
    }
}
