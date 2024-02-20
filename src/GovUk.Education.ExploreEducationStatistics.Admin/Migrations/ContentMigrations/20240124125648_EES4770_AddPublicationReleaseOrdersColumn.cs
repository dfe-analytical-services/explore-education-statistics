using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES4770_AddPublicationReleaseOrdersColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "Releases");

            migrationBuilder.AddColumn<string>(
                name: "ReleaseOrders",
                table: "Publications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReleaseOrders",
                table: "Publications");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Releases",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
