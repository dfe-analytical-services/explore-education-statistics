using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class AddReleaseType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TypeId",
                table: "Releases",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ReleaseTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ReleaseTypes",
                columns: new[] { "Id", "Title" },
                values: new object[] { new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"), "Official Statistics" });

            migrationBuilder.InsertData(
                table: "ReleaseTypes",
                columns: new[] { "Id", "Title" },
                values: new object[] { new Guid("1821abb8-68b0-431b-9770-0bea65d02ff0"), "Ad Hoc" });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "TypeId",
                value: new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"));

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                column: "TypeId",
                value: new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"));

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "TypeId",
                value: new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"));

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                column: "TypeId",
                value: new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"));

            migrationBuilder.CreateIndex(
                name: "IX_Releases_TypeId",
                table: "Releases",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Releases_ReleaseTypes_TypeId",
                table: "Releases",
                column: "TypeId",
                principalTable: "ReleaseTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Releases_ReleaseTypes_TypeId",
                table: "Releases");

            migrationBuilder.DropTable(
                name: "ReleaseTypes");

            migrationBuilder.DropIndex(
                name: "IX_Releases_TypeId",
                table: "Releases");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Releases");
        }
    }
}
