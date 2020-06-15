using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES977DifferentiateTitlesAndSlugs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("1d0e4263-3d70-433e-bd95-f29754db5888"),
                columns: new[] { "Slug", "Title" },
                values: new object[] { "multi-academy-trust-performance-measures-at-ks4", "Multi-academy trust performance measures at key stage 4" });

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("eab51107-4ef0-4926-8f8b-c8bd7f5a21d5"),
                columns: new[] { "Slug", "Title" },
                values: new object[] { "multi-academy-trust-performance-measures-at-ks2", "Multi-academy trust performance measures at key stage 2" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("1d0e4263-3d70-433e-bd95-f29754db5888"),
                columns: new[] { "Slug", "Title" },
                values: new object[] { "multi-academy-trust-performance-measures", "Multi-academy trust performance measures" });

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("eab51107-4ef0-4926-8f8b-c8bd7f5a21d5"),
                columns: new[] { "Slug", "Title" },
                values: new object[] { "multi-academy-trust-performance-measures", "Multi-academy trust performance measures" });
        }
    }
}
