using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class RemoveUnusedPublicationNextUpdateDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextUpdate",
                table: "Publications");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "NextReleaseDate",
                value: "{\"Year\":\"2019\",\"Month\":\"3\",\"Day\":\"22\"}");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                column: "NextReleaseDate",
                value: "{\"Year\":\"2019\",\"Month\":\"6\",\"Day\":\"14\"}");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "NextReleaseDate",
                value: "{\"Year\":\"2019\",\"Month\":\"7\",\"Day\":\"19\"}");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NextUpdate",
                table: "Publications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"),
                column: "NextUpdate",
                value: new DateTime(2019, 6, 14, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                column: "NextUpdate",
                value: new DateTime(2019, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                column: "NextUpdate",
                value: new DateTime(2019, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "NextReleaseDate",
                value: null);

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                column: "NextReleaseDate",
                value: null);

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "NextReleaseDate",
                value: null);
        }
    }
}
