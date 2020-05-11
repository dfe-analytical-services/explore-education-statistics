using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES813UpdatingContact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("d246c696-4b3a-4aeb-842c-c1318ee334e8"),
                columns: new[] { "ContactName", "ContactTelNo" },
                values: new object[] { "Sean Gibson", "0370 000 2288" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("d246c696-4b3a-4aeb-842c-c1318ee334e8"),
                columns: new[] { "ContactName", "ContactTelNo" },
                values: new object[] { "Mark Pearson", "01142742585" });
        }
    }
}
