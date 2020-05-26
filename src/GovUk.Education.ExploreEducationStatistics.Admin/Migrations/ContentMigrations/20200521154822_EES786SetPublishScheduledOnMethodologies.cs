using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES786SetPublishScheduledOnMethodologies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("8ab41234-cc9d-4b3d-a42c-c9fce7762719"),
                column: "PublishScheduled",
                value: new DateTime(2018, 6, 13, 23, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("c8c911e3-39c1-452b-801f-25bb79d1deb7"),
                column: "PublishScheduled",
                value: new DateTime(2018, 8, 24, 23, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("caa8e56f-41d2-4129-a5c3-53b051134bd7"),
                column: "PublishScheduled",
                value: new DateTime(2018, 3, 22, 0, 0, 0, 0, DateTimeKind.Utc));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("8ab41234-cc9d-4b3d-a42c-c9fce7762719"),
                column: "PublishScheduled",
                value: null);

            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("c8c911e3-39c1-452b-801f-25bb79d1deb7"),
                column: "PublishScheduled",
                value: null);

            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("caa8e56f-41d2-4129-a5c3-53b051134bd7"),
                column: "PublishScheduled",
                value: null);
        }
    }
}
