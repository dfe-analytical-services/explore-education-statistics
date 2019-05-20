using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class MethodologyExampleContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Methodologies",
                columns: new[] { "Id", "Annexes", "Content", "PublicationId", "Published", "Summary", "Title" },
                values: new object[] { new Guid("caa8e56f-41d2-4129-a5c3-53b051134bd7"), "[{\"Order\":1,\"Heading\":\"Annex A - Glossary\",\"Caption\":\"\",\"Content\":[]},{\"Order\":2,\"Heading\":\"Annex B - Calculations\",\"Caption\":\"\",\"Content\":[]},{\"Order\":3,\"Heading\":\"Annex C - School attendance codes\",\"Caption\":\"\",\"Content\":[]},{\"Order\":4,\"Heading\":\"Annex D - Links to pupil absence national statistics and data\",\"Caption\":\"\",\"Content\":[]},{\"Order\":5,\"Heading\":\"Annex E - Standard breakdowns\",\"Caption\":\"\",\"Content\":[]},{\"Order\":6,\"Heading\":\"Annex F - Timeline\",\"Caption\":\"\",\"Content\":[]}]", "[{\"Order\":1,\"Heading\":\"1. Overview of absence statistics\",\"Caption\":\"\",\"Content\":[]},{\"Order\":2,\"Heading\":\"2. National Statistics badging\",\"Caption\":\"\",\"Content\":[]},{\"Order\":3,\"Heading\":\"3. Methodology\",\"Caption\":\"\",\"Content\":[]},{\"Order\":4,\"Heading\":\"4. Data collection\",\"Caption\":\"\",\"Content\":[]},{\"Order\":5,\"Heading\":\"5. Data processing\",\"Caption\":\"\",\"Content\":[]},{\"Order\":6,\"Heading\":\"6. Data quality\",\"Caption\":\"\",\"Content\":[]},{\"Order\":7,\"Heading\":\"7. Contacts\",\"Caption\":\"\",\"Content\":[]}]", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), new DateTime(2018, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Find out about the methodology behind pupil absence statistics and data and how and why they're collected and published.", "Pupil absence statistics: methodology" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("caa8e56f-41d2-4129-a5c3-53b051134bd7"));
        }
    }
}
