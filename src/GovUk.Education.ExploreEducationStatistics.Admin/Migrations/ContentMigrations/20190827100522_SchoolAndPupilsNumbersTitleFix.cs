using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    [ExcludeFromCodeCoverage]
    public partial class SchoolAndPupilsNumbersTitleFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                column: "Title",
                value: "Schools, pupils and their characteristics");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                column: "Title",
                value: "School and pupils and their characteristics");
        }
    }
}
