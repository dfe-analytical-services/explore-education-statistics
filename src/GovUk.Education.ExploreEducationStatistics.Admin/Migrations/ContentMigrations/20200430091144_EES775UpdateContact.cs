using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES775UpdateContact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("ee8b0c92-b556-4670-904b-c265f0332a9e"),
                columns: new[] { "ContactName", "ContactTelNo" },
                values: new object[] { "Daisy Astill", "07741 118332" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("ee8b0c92-b556-4670-904b-c265f0332a9e"),
                columns: new[] { "ContactName", "ContactTelNo" },
                values: new object[] { "Matthew Bridge", "07384 456648" });
        }
    }
}
