using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES913AddUpdatedFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastUpdated",
                table: "Methodologies",
                newName: "Updated");

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "Publications",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Updated",
                table: "Publications");

            migrationBuilder.RenameColumn(
                name: "Updated",
                table: "Methodologies",
                newName: "LastUpdated");
        }
    }
}