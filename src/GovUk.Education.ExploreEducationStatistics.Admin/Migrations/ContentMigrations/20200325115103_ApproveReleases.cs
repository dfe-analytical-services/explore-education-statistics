using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class ApproveReleases : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                columns: new[] { "PublishScheduled", "Published", "Status" },
                values: new object[] { new DateTime(2018, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2018, 4, 25, 9, 30, 0, 0, DateTimeKind.Unspecified), "Approved" });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                columns: new[] { "PublishScheduled", "Published" },
                values: new object[] { new DateTime(2018, 6, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), null });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                columns: new[] { "PublishScheduled", "Published", "Status" },
                values: new object[] { new DateTime(2018, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2018, 7, 19, 9, 30, 0, 0, DateTimeKind.Unspecified), "Approved" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                columns: new[] { "PublishScheduled", "Published", "Status" },
                values: new object[] { null, new DateTime(2018, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Draft" });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                columns: new[] { "PublishScheduled", "Published" },
                values: new object[] { null, new DateTime(2018, 6, 14, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                columns: new[] { "PublishScheduled", "Published", "Status" },
                values: new object[] { null, new DateTime(2018, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Draft" });
        }
    }
}
