using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class AddUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Update",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<Guid>(nullable: false),
                    On = table.Column<DateTime>(nullable: false),
                    Reason = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Update", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Update_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "Published",
                value: new DateTime(2017, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                column: "Published",
                value: new DateTime(2016, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("9c0f0139-7f88-4750-afe0-1c85cdf1d047"), new DateTime(2017, 4, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Underlying data file updated to include absence data by pupil residency and school location, andupdated metadata document.", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("18e0d40e-bdf7-4c84-99dd-732e72e9c9a5"), new DateTime(2017, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "First published.", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("51bd1e2f-2669-4708-b300-799b6be9ec9a"), new DateTime(2016, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "First published.", new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d") });

            migrationBuilder.CreateIndex(
                name: "IX_Update_ReleaseId",
                table: "Update",
                column: "ReleaseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Update");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "Published",
                value: new DateTime(2018, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                column: "Published",
                value: new DateTime(2018, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
