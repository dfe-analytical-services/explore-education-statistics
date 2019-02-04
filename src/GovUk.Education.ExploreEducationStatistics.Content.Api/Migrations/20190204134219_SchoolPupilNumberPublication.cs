using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class SchoolPupilNumberPublication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("6404a25c-7352-4887-aa0e-c62948d45b57"), "January 2017", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2017" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("31b06d27-7e31-491f-bd5d-89105369ac60"), "January 2016", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2016" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("e764ef78-97f6-406c-aaaf-d3a5c847b362"), "January 2015", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2015" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("1479a18c-b5a6-47c5-bb48-c8d60084b1a4"), "January 2014", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2014" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("86893fdd-4a24-4fe9-9902-02ad8bbf8632"), "January 2013", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2013" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("95c24ea7-4ebe-4f73-88f1-91ea33ec00bf"), "January 2012", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2012" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("758b83d6-bd47-44aa-8ee2-359f350fef0a"), "January 2011", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2011" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("acbef79a-7b53-492e-a679-ca994edfc892"), "January 2010", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2010" });

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                column: "Summary",
                value: "Statistics on the number and characteristics of schools and pupils.");

            migrationBuilder.InsertData(
                table: "Releases",
                columns: new[] { "Id", "PublicationId", "Published", "ReleaseName", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"), new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), new DateTime(2018, 5, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "January 2018 ", "january-2018", "Statistics on pupils in schools in England as collected in the January 2018 school census.", " Schools, pupils and their characteristics: January 2018 " });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("9aab1af8-27d4-43c4-a7cd-afb375c8809c"), new DateTime(2018, 5, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "First published.", new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("aa4c0f33-cdf4-4df9-9540-18472d46a301"), new DateTime(2018, 6, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Amended title of table 8e in attachment 'Schools pupils and their characteristics 2018 - LA tables'.", new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("4bd0f73b-ef2b-4901-839a-80cbf8c0871f"), new DateTime(2018, 7, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Removed unrelated extra material from table 7c in attachment 'Schools pupils and their characteristics 2018 - LA tables'.", new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("7f911a4e-7a56-4f6f-92a6-bd556a9bcfd3"), new DateTime(2018, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Added cross-border movement local authority level and underlying data tables.", new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("d008b331-af29-4c7e-bb8a-5a2005aa0131"), new DateTime(2018, 9, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Added open document version of 'Schools pupils and their characteristics 2018 - Cross-border movement local authority tables'.", new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c") });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("1479a18c-b5a6-47c5-bb48-c8d60084b1a4"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("31b06d27-7e31-491f-bd5d-89105369ac60"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("6404a25c-7352-4887-aa0e-c62948d45b57"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("758b83d6-bd47-44aa-8ee2-359f350fef0a"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("86893fdd-4a24-4fe9-9902-02ad8bbf8632"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("95c24ea7-4ebe-4f73-88f1-91ea33ec00bf"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("acbef79a-7b53-492e-a679-ca994edfc892"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("e764ef78-97f6-406c-aaaf-d3a5c847b362"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("4bd0f73b-ef2b-4901-839a-80cbf8c0871f"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("7f911a4e-7a56-4f6f-92a6-bd556a9bcfd3"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("9aab1af8-27d4-43c4-a7cd-afb375c8809c"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("aa4c0f33-cdf4-4df9-9540-18472d46a301"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("d008b331-af29-4c7e-bb8a-5a2005aa0131"));

            migrationBuilder.DeleteData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                column: "Summary",
                value: "Lorem ipsum dolor sit amet.");
        }
    }
}
