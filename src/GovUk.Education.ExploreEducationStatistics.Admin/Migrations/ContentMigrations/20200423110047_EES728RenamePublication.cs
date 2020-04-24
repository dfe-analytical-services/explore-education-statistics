using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES728RenamePublication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("88312cc0-fe1d-4ab5-81df-33fd708185cb"),
                columns: new[] { "Slug", "Title" },
                values: new object[] { "education-health-and-care-plans", "Education, health and care plans" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("88312cc0-fe1d-4ab5-81df-33fd708185cb"),
                columns: new[] { "Slug", "Title" },
                values: new object[] { "statements-on-sen-and-ehc-plans", "Statements on SEN and EHC plans" });
        }
    }
}
