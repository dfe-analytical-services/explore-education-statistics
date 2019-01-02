using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class Slugs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Topics",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Themes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Publications",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                column: "Slug",
                value: "pupil-absence-in-schools-in-england");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"),
                column: "Slug",
                value: "schools");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("bc08839f-2970-4f34-af2d-29608a48082f"),
                column: "Slug",
                value: "16+");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"),
                column: "Slug",
                value: "schools");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("0b920c62-ff67-4cf1-89ec-0c74a364e6b4"),
                column: "Slug",
                value: "number-of-children");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"),
                column: "Slug",
                value: "absence-and-exclusions");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"),
                column: "Slug",
                value: "results");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"),
                column: "Slug",
                value: "school-and-pupil-numbers");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("3bef5b2b-76a1-4be1-83b1-a3269245c610"),
                column: "Slug",
                value: "vulnerable-children");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("4c658598-450b-4493-b972-8812acd154a7"),
                column: "Slug",
                value: "higher-education");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("66ff5e67-36cf-4210-9ad2-632baeb4eca7"),
                column: "Slug",
                value: "school-finance");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"),
                column: "Slug",
                value: "further-education");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5"),
                column: "Slug",
                value: "capacity-admissions");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("d5288137-e703-43a1-b634-d50fc9785cb9"),
                column: "Slug",
                value: "teacher-numbers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Publications");
        }
    }
}
