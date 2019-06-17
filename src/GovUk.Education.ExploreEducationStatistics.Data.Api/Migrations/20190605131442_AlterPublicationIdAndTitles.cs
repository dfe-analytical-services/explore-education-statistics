using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class AlterPublicationIdAndTitles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                column: "Title",
                value: "Pupil absence in schools in England");

            migrationBuilder.UpdateData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f"),
                column: "Title",
                value: "Early years foundation stage profile results");

            migrationBuilder.InsertData(
                table: "Publication",
                columns: new[] { "Id", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "permanent-and-fixed-period-exclusions-in-england", "Permanent and fixed-period exclusions in England", new Guid("77941b7d-bbd6-4069-9107-565af89e2dec") });

            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "PublicationId",
                value: new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"));
            
            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("8345e27a-7a32-4b20-a056-309163bdf9c4"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Publication",
                columns: new[] { "Id", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("8345e27a-7a32-4b20-a056-309163bdf9c4"), "permanent-and-fixed-period-exclusions-in-england", "Permanent and fixed-period exclusions in England", new Guid("77941b7d-bbd6-4069-9107-565af89e2dec") });

            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "PublicationId",
                value: new Guid("8345e27a-7a32-4b20-a056-309163bdf9c4"));
            
            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"));

            migrationBuilder.UpdateData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                column: "Title",
                value: "Pupil absence data and statistics for schools in England");

            migrationBuilder.UpdateData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f"),
                column: "Title",
                value: "Early years foundation stage profile data");
        }
    }
}
