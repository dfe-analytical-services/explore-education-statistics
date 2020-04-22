using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class RenameNEETStatPublication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2e510281-ca8c-41bf-bbe0-fd15fcc81aae"),
                columns: new[] { "Slug", "Title" },
                values: new object[] { "neet-statistics-annual-brief", "NEET statistics annual brief" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2e510281-ca8c-41bf-bbe0-fd15fcc81aae"),
                columns: new[] { "Slug", "Title" },
                values: new object[] { "neet-statistics-quarterly-brief", "NEET statistics quarterly brief" });
        }
    }
}
