using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES825RevertSlug : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("42a888c4-9ee7-40fd-9128-f5de546780b3"),
                column: "Slug",
                value: "graduate-labour-markets");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("42a888c4-9ee7-40fd-9128-f5de546780b3"),
                column: "Slug",
                value: "graduate-labour-market-statistics");
        }
    }
}
