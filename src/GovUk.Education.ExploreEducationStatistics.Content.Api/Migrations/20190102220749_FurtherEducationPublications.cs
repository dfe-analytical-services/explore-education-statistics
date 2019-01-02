using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class FurtherEducationPublications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "Description", "NextUpdate", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("70902b3c-0bb4-457d-b40a-2a959cdc7d00"), null, null, "16-to-18-school-performance", "16 to 18 school performance", new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "Description", "NextUpdate", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("d0e56978-c944-4b12-9156-bfe50c94c2a0"), null, null, "destination-of-leavers", "Destination of leavers", new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "Description", "NextUpdate", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("ad81ebdd-2bbc-47e8-a32c-f396d6e2bb72"), null, null, "further-education-and-skills", "Further education and skills", new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "Description", "NextUpdate", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("201cb72d-ef35-4680-ade7-b09a8dca9cc1"), null, null, "apprenticeship-and-levy-statistics", "Apprenticeship and levy statistics", new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "Description", "NextUpdate", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("7bd128a3-ae7f-4e1b-984e-d1b795c61630"), null, null, "apprenticeships-and-traineeships", "Apprenticeships and traineeships", new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860") });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("201cb72d-ef35-4680-ade7-b09a8dca9cc1"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("70902b3c-0bb4-457d-b40a-2a959cdc7d00"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("7bd128a3-ae7f-4e1b-984e-d1b795c61630"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("ad81ebdd-2bbc-47e8-a32c-f396d6e2bb72"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d0e56978-c944-4b12-9156-bfe50c94c2a0"));
        }
    }
}
