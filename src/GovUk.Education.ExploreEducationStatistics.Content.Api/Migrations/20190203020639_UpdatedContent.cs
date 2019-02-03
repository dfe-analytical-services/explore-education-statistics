using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class UpdatedContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Links_Publications_PublicationId",
                table: "Links");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Links",
                table: "Links");

            migrationBuilder.RenameTable(
                name: "Links",
                newName: "Link");

            migrationBuilder.RenameIndex(
                name: "IX_Links_PublicationId",
                table: "Link",
                newName: "IX_Link_PublicationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Link",
                table: "Link",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                column: "Summary",
                value: "View statistics, create charts and tables and download data files for fixed-period and permanent exclusion statistics");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                column: "Summary",
                value: "View statistics, create charts and tables and download data files for authorised, overall, persistent and unauthorised absence");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("bc08839f-2970-4f34-af2d-29608a48082f"),
                columns: new[] { "Slug", "Title" },
                values: new object[] { "higher-education", "Higher education" });

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"),
                columns: new[] { "Slug", "Title" },
                values: new object[] { "early-years-and-schools", "Early years and schools" });

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"),
                column: "Summary",
                value: "Pupil absence and permanent and fixed-period exclusions statistics and data");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"),
                column: "Summary",
                value: "Local authority and school finance");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("66ff5e67-36cf-4210-9ad2-632baeb4eca7"),
                column: "Summary",
                value: "Local authority and school finance");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5"),
                column: "Summary",
                value: "School capacity, admission appeals");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("d5288137-e703-43a1-b634-d50fc9785cb9"),
                column: "Summary",
                value: "The number and characteristics of teachers");

            migrationBuilder.AddForeignKey(
                name: "FK_Link_Publications_PublicationId",
                table: "Link",
                column: "PublicationId",
                principalTable: "Publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Link_Publications_PublicationId",
                table: "Link");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Link",
                table: "Link");

            migrationBuilder.RenameTable(
                name: "Link",
                newName: "Links");

            migrationBuilder.RenameIndex(
                name: "IX_Link_PublicationId",
                table: "Links",
                newName: "IX_Links_PublicationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Links",
                table: "Links",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                column: "Summary",
                value: "Permanent exclusions, fixed period exclusions");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                column: "Summary",
                value: "Overall absence, Authorised absence, Unauthorised absence, Persistence absence");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("bc08839f-2970-4f34-af2d-29608a48082f"),
                columns: new[] { "Slug", "Title" },
                values: new object[] { "16+", "16+" });

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"),
                columns: new[] { "Slug", "Title" },
                values: new object[] { "schools", "Schools" });

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"),
                column: "Summary",
                value: "Pupil absence, permanent and fixed period exlusions");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"),
                column: "Summary",
                value: "Schools, pupils and their characteristics, SEN and EHC plans, SEN in England");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("66ff5e67-36cf-4210-9ad2-632baeb4eca7"),
                column: "Summary",
                value: "Schools, pupils and their characteristics, SEN and EHC plans, SEN in England");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5"),
                column: "Summary",
                value: "School capacity, Admission appeals");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("d5288137-e703-43a1-b634-d50fc9785cb9"),
                column: "Summary",
                value: "School capacity, Admission appeals");

            migrationBuilder.AddForeignKey(
                name: "FK_Links_Publications_PublicationId",
                table: "Links",
                column: "PublicationId",
                principalTable: "Publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
