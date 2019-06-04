using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class AddThemeSummaries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("060c5376-35d8-420b-8266-517a9339b7bc"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-providers-survey");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("0ce6a6c6-5451-4967-8dd4-2f4fa8131982"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-childcare-and-early-years#provision-for-children-under-5-years-of-age-in-england");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("89869bba-0c00-40f7-b7d6-e28cb904ad37"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-children-in-need#characteristics-of-children-in-need");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a0eb117e-44a8-4732-adf1-8fbc890cbb62"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-neet#participation-in-education");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                column: "LegacyPublicationUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("eab51107-4ef0-4926-8f8b-c8bd7f5a21d5"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-key-stage-2#national-curriculum-assessments-at-key-stage-2");

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
                columns: new[] { "Summary", "Title" },
                values: new object[] { "Including graduate labour market and not in education, employment or training (NEET) statistics", "Destination of pupils and students" });

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("74648781-85a9-4233-8be3-fe6f137165f4"),
                columns: new[] { "Summary", "Title" },
                values: new object[] { "Including GCSE and key stage statistcs", "School and college outcomes and performance" });

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("92c5df93-c4da-4629-ab25-51bd2920cdca"),
                column: "Summary",
                value: "Including advanced learner loan, benefit claimant and apprenticeship and traineeship statistics");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("a95d2ca2-a969-4320-b1e9-e4781112574a"),
                column: "Summary",
                value: "Including summarised expenditure, post-compulsory education, qualitification and school statistics");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("b601b9ea-b1c7-4970-b354-d1f695c446f1"),
                column: "Summary",
                value: "Including initial teacher training (ITT) statistics");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("bc08839f-2970-4f34-af2d-29608a48082f"),
                column: "Summary",
                value: "Including local authority (LA) and student loan statistics");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"),
                columns: new[] { "Summary", "Title" },
                values: new object[] { "Including children in need, EYFS, and looked after children and social workforce statistics", "Children, early years and social care" });

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"),
                column: "Summary",
                value: "Including absence, application and offers, capacity exclusion and special educational needs (SEN) statistics");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("060c5376-35d8-420b-8266-517a9339b7bc"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-survey-of-parents");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("0ce6a6c6-5451-4967-8dd4-2f4fa8131982"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-survey-of-parents");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("89869bba-0c00-40f7-b7d6-e28cb904ad37"),
                column: "LegacyPublicationUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a0eb117e-44a8-4732-adf1-8fbc890cbb62"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-neet#participation-in-education,-employment-or-training");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-school-and-pupil-numbers#documents");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("eab51107-4ef0-4926-8f8b-c8bd7f5a21d5"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-key-stage-2#pupil-attainment-at-key-stage-2");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("2ca22e34-b87a-4281-a0eb-b80f4f8dd374"),
                column: "Summary",
                value: "");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"),
                columns: new[] { "Summary", "Title" },
                values: new object[] { "", "Destination of pupils and students - including NEET" });

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("74648781-85a9-4233-8be3-fe6f137165f4"),
                columns: new[] { "Summary", "Title" },
                values: new object[] { "", "School and college performance - include GCSE and key stage results" });

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("92c5df93-c4da-4629-ab25-51bd2920cdca"),
                column: "Summary",
                value: "");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("a95d2ca2-a969-4320-b1e9-e4781112574a"),
                column: "Summary",
                value: "");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("b601b9ea-b1c7-4970-b354-d1f695c446f1"),
                column: "Summary",
                value: "");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("bc08839f-2970-4f34-af2d-29608a48082f"),
                column: "Summary",
                value: "");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"),
                columns: new[] { "Summary", "Title" },
                values: new object[] { "", "Children and early years - including social care" });

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"),
                column: "Summary",
                value: "");
        }
    }
}
