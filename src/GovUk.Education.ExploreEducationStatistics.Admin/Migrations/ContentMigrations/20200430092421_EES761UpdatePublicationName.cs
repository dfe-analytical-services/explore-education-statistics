using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES761UpdatePublicationName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("4d29c28c-efd1-4245-a80c-b55c6a50e3f7"),
                columns: new[] { "Slug", "Title" },
                values: new object[] { "graduate-outcomes-leo-postgraduate-outcomes", "Graduate outcomes (LEO): postgraduate outcomes" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("4d29c28c-efd1-4245-a80c-b55c6a50e3f7"),
                columns: new[] { "Slug", "Title" },
                values: new object[] { "graduate-outcomes", "Graduate outcomes (LEO)" });
        }
    }
}
