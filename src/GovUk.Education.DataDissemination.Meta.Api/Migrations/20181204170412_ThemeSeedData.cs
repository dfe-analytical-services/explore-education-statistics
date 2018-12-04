using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.DataDissemination.Meta.Api.Migrations
{
    public partial class ThemeSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Themes",
                columns: new[] { "Id", "Title" },
                values: new object[] { new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "Schools" });

            migrationBuilder.InsertData(
                table: "Themes",
                columns: new[] { "Id", "Title" },
                values: new object[] { new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"), "Social Care" });

            migrationBuilder.InsertData(
                table: "Themes",
                columns: new[] { "Id", "Title" },
                values: new object[] { new Guid("bc08839f-2970-4f34-af2d-29608a48082f"), "16+" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"));

            migrationBuilder.DeleteData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("bc08839f-2970-4f34-af2d-29608a48082f"));

            migrationBuilder.DeleteData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"));
        }
    }
}
