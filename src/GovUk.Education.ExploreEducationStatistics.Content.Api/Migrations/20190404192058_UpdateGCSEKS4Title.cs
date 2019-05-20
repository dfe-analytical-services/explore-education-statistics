using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class UpdateGCSEKS4Title : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"),
                columns: new[] { "Slug", "Summary", "Title" },
                values: new object[] { "gcse-and-equivalent-results-in-england", "View statistics, create charts and tables and download data files for GCSE and equivalent results in England", "GCSE and equivalent results in England" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"),
                columns: new[] { "Slug", "Summary", "Title" },
                values: new object[] { "ks4-statistics", "Lorem ipsum dolor sit amet.", "KS4 statistics" });
        }
    }
}
