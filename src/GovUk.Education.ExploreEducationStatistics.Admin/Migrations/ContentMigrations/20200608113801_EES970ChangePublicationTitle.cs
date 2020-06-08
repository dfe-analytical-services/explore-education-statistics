using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES970ChangePublicationTitle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"),
                columns: new[] { "Slug", "Title" },
                values: new object[] { "secondary-and-primary-school-applications-and-offers", "Secondary and primary school applications and offers" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"),
                columns: new[] { "Slug", "Title" },
                values: new object[] { "secondary-and-primary-schools-applications-and-offers", "Secondary and primary schools applications and offers" });
        }
    }
}
