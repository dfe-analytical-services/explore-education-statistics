using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class AddAdditionalMethodologies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Methodologies",
                columns: new[] { "Id", "Annexes", "Content", "PublicationId", "Published", "Summary", "Title" },
                values: new object[] { new Guid("8ab41234-cc9d-4b3d-a42c-c9fce7762719"), "[]", "[]", new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"), new DateTime(2018, 6, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "School application statistics: methodology" });

            migrationBuilder.InsertData(
                table: "Methodologies",
                columns: new[] { "Id", "Annexes", "Content", "PublicationId", "Published", "Summary", "Title" },
                values: new object[] { new Guid("c8c911e3-39c1-452b-801f-25bb79d1deb7"), "[]", "[]", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), new DateTime(2018, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "Pupil exclusion statistics: methodology" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("8ab41234-cc9d-4b3d-a42c-c9fce7762719"));

            migrationBuilder.DeleteData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("c8c911e3-39c1-452b-801f-25bb79d1deb7"));
        }
    }
}
