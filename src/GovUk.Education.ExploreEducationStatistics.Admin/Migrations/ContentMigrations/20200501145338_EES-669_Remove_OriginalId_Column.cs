using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES669_Remove_OriginalId_Column : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Releases_Releases_OriginalId",
                table: "Releases");

            migrationBuilder.DropIndex(
                name: "IX_Releases_OriginalId_Version",
                table: "Releases");

            migrationBuilder.DropColumn(
                name: "OriginalId",
                table: "Releases");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "PreviousVersionId",
                value: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"));

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                column: "PreviousVersionId",
                value: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"));

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "PreviousVersionId",
                value: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OriginalId",
                table: "Releases",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                columns: new[] { "OriginalId", "PreviousVersionId" },
                values: new object[] { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                columns: new[] { "OriginalId", "PreviousVersionId" },
                values: new object[] { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                columns: new[] { "OriginalId", "PreviousVersionId" },
                values: new object[] { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.CreateIndex(
                name: "IX_Releases_OriginalId_Version",
                table: "Releases",
                columns: new[] { "OriginalId", "Version" });

            migrationBuilder.AddForeignKey(
                name: "FK_Releases_Releases_OriginalId",
                table: "Releases",
                column: "OriginalId",
                principalTable: "Releases",
                principalColumn: "Id");
        }
    }
}
