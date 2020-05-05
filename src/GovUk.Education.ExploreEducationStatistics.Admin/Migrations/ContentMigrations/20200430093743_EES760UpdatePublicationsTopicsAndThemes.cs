using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES760UpdatePublicationsTopicsAndThemes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("42a888c4-9ee7-40fd-9128-f5de546780b3"),
                columns: new[] {"Slug", "TopicId"},
                values: new object[] { "graduate-outcomes-leo-postgraduate-outcomes", new Guid("53a1fbb7-5234-435f-892b-9baad4c82535") });

            migrationBuilder.DeleteData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("3bef5b2b-76a1-4be1-83b1-a3269245c610"));

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("2ca22e34-b87a-4281-a0eb-b80f4f8dd374"),
                column: "Summary",
                value: "Including university graduate employment, graduate labour market and participation statistics");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"),
                column: "Summary",
                value: "Including not in education, employment or training (NEET) statistics");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("2ca22e34-b87a-4281-a0eb-b80f4f8dd374"),
                column: "Summary",
                value: "Including university graduate employment and participation statistics");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"),
                column: "Summary",
                value: "Including graduate labour market and not in education, employment or training (NEET) statistics");

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "Description", "Slug", "Summary", "ThemeId", "Title" },
                values: new object[] { new Guid("3bef5b2b-76a1-4be1-83b1-a3269245c610"), null, "graduate-labour-market", "", new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"), "Graduate labour market" });

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("42a888c4-9ee7-40fd-9128-f5de546780b3"),
                columns: new[] { "Slug", "TopicId" },
                values: new object[] { "graduate-labour-markets", new Guid("3bef5b2b-76a1-4be1-83b1-a3269245c610") });
        }
    }
}
