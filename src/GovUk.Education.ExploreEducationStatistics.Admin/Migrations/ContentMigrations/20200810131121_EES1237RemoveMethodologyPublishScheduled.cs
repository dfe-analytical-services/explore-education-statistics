using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1237RemoveMethodologyPublishScheduled : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishScheduled",
                table: "Methodologies");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PublishScheduled",
                table: "Methodologies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("caa8e56f-41d2-4129-a5c3-53b051134bd7"),
                column: "PublishScheduled",
                value: new DateTime(2018, 3, 22, 0, 0, 0, 0, DateTimeKind.Utc));
        }
    }
}
