using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class AddSummaryField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Topics",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Themes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Publications",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("143c672b-18d7-478b-a6e7-b843c9b3fd42"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("201cb72d-ef35-4680-ade7-b09a8dca9cc1"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("526dea0e-abf3-476e-9ca4-9dbd9b101bc8"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("70902b3c-0bb4-457d-b40a-2a959cdc7d00"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("7bd128a3-ae7f-4e1b-984e-d1b795c61630"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("8b2c1269-3495-4f89-83eb-524fc0b6effc"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("9674ac24-649a-400c-8a2c-871793d9cd7a"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a20ea465-d2d0-4fc1-96ee-6b2ca4e0520e"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a4b22113-47d3-48fc-b2da-5336c801a31f"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("ad81ebdd-2bbc-47e8-a32c-f396d6e2bb72"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bd781dc5-cfc7-4543-b8d7-a3a7b3606b3d"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                column: "Summary",
                value: "Permanent exclusions, fixed period exclusions");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                column: "Summary",
                value: "Overall absence, Authorised absence, Unauthorised absence, Persistence absence");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d04142bd-f448-456b-97bc-03863143836b"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d0e56978-c944-4b12-9156-bfe50c94c2a0"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("fe94b33d-0419-4fac-bf73-28299d5e4247"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("bc08839f-2970-4f34-af2d-29608a48082f"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("0b920c62-ff67-4cf1-89ec-0c74a364e6b4"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"),
                columns: new[] { "Summary", "Title" },
                values: new object[] { "Pupil absence, permanent and fixed period exlusions", "Absence and exclusions" });

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"),
                column: "Summary",
                value: "Schools, pupils and their characteristics, SEN and EHC plans, SEN in England");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"),
                columns: new[] { "Summary", "Title" },
                values: new object[] { "Schools, pupils and their characteristics, SEN and EHC plans, SEN in England", "School & pupil numbers" });

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("3bef5b2b-76a1-4be1-83b1-a3269245c610"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("4c658598-450b-4493-b972-8812acd154a7"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("66ff5e67-36cf-4210-9ad2-632baeb4eca7"),
                columns: new[] { "Summary", "Title" },
                values: new object[] { "Schools, pupils and their characteristics, SEN and EHC plans, SEN in England", "School finance" });

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5"),
                columns: new[] { "Summary", "Title" },
                values: new object[] { "School capacity, Admission appeals", "Capacity and admissions" });

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("d5288137-e703-43a1-b634-d50fc9785cb9"),
                column: "Summary",
                value: "School capacity, Admission appeals");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Publications");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"),
                column: "Title",
                value: "Absence and Exclusions");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"),
                column: "Title",
                value: "School and Pupil Numbers");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("66ff5e67-36cf-4210-9ad2-632baeb4eca7"),
                column: "Title",
                value: "School Finance");

            migrationBuilder.UpdateData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5"),
                column: "Title",
                value: "Capacity Admissions");
        }
    }
}
