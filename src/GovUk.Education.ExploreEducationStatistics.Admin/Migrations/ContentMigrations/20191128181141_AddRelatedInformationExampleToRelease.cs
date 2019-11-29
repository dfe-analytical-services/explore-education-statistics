using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class AddRelatedInformationExampleToRelease : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "RelatedInformation",
                value: "[{\"Id\":\"f3c67bc9-6132-496e-a848-c39dfcd16f49\",\"Description\":\"Additional guidance\",\"Url\":\"http://example.com\"},{\"Id\":\"45acb50c-8b21-46b4-989f-36f4b0ee37fb\",\"Description\":\"Statistics guide\",\"Url\":\"http://example.com\"}]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "RelatedInformation",
                value: null);
        }
    }
}
