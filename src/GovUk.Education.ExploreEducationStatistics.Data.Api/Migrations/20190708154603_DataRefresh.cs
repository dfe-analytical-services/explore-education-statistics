using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class DataRefresh : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Theme",
                columns: new[] { "Id", "Slug", "Title" },
                values: new object[] { new Guid("9aa81762-e52c-40d4-8a90-f469977360a7"), "further-education", "Further education" });

            migrationBuilder.InsertData(
                table: "Theme",
                columns: new[] { "Id", "Slug", "Title" },
                values: new object[] { new Guid("fe805471-17e9-4ac6-a555-c7d0ebec1b90"), "outcomes-and-performance", "School and college outcomes and performance" });

            migrationBuilder.InsertData(
                table: "Topic",
                columns: new[] { "Id", "Slug", "ThemeId", "Title" },
                values: new object[] { new Guid("dfc908db-242a-4e3a-b6c6-e3f66cd152af"), "sen", new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"), "Special educational needs (SEN)" });

            migrationBuilder.InsertData(
                table: "Publication",
                columns: new[] { "Id", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("2d94e5c8-a272-497c-bda0-c1f6b75155b0"), "statements-of-sen-and-ehc-plans", "Statements of SEN and EHC plans", new Guid("dfc908db-242a-4e3a-b6c6-e3f66cd152af") });

            migrationBuilder.InsertData(
                table: "Topic",
                columns: new[] { "Id", "Slug", "ThemeId", "Title" },
                values: new object[,]
                {
                    { new Guid("721048b9-8c06-4bad-8585-8789fa38a03b"), "national-achievement-rates-tables", new Guid("9aa81762-e52c-40d4-8a90-f469977360a7"), "National achievement rates tables" },
                    { new Guid("71444ff6-614f-405b-b6c7-f72077d42e34"), "further-education-and-skills", new Guid("9aa81762-e52c-40d4-8a90-f469977360a7"), "Further education and skills" },
                    { new Guid("9e4fa097-2999-4c4d-9ecd-0c4733fc71b4"), "sixteen-to-nineteen-attainment", new Guid("fe805471-17e9-4ac6-a555-c7d0ebec1b90"), "16 to 19 attainment" },
                    { new Guid("f38469bd-a5f7-46b1-96bb-3b0a01e9e53f"), "key-stage-two", new Guid("fe805471-17e9-4ac6-a555-c7d0ebec1b90"), "Key stage 2" },
                    { new Guid("81fbb21d-3c49-46a2-8b43-0076974114f7"), "key-stage-four", new Guid("fe805471-17e9-4ac6-a555-c7d0ebec1b90"), "GCSEs (key stage 4)" }
                });

            migrationBuilder.InsertData(
                table: "Publication",
                columns: new[] { "Id", "Slug", "Title", "TopicId" },
                values: new object[,]
                {
                    { new Guid("99ce35fb-3fe2-48bb-9b73-23159df9d5ea"), "national-achievement-rates-tables", "National achievement rates tables", new Guid("721048b9-8c06-4bad-8585-8789fa38a03b") },
                    { new Guid("5aea252e-fddc-42b3-a1da-47f12e523e70"), "apprenticeships-and-traineeships", "Apprenticeships and traineeships", new Guid("71444ff6-614f-405b-b6c7-f72077d42e34") },
                    { new Guid("d5a01a5c-cd57-482f-8a19-803b266e1012"), "further-education-and-skills", "Further education and skills", new Guid("71444ff6-614f-405b-b6c7-f72077d42e34") },
                    { new Guid("adb95888-64c7-4aa7-ba70-a9e535f8a30f"), "Level 2 and 3 attainment by young people aged 19", "Level 2 and 3 attainment by young people aged 19", new Guid("9e4fa097-2999-4c4d-9ecd-0c4733fc71b4") },
                    { new Guid("90ecb3d3-bd05-4f84-a73e-1d153568b320"), "national-curriculum-assessments-key-stage2", "National curriculum assessments at key stage 2", new Guid("f38469bd-a5f7-46b1-96bb-3b0a01e9e53f") },
                    { new Guid("15659c96-a624-4457-846d-2ab5f3db6aec"), "gcse-results-including-pupil-characteristics", "GCSE and equivalent results, including pupil characteristics", new Guid("81fbb21d-3c49-46a2-8b43-0076974114f7") }
                });

            migrationBuilder.InsertData(
                table: "Release",
                columns: new[] { "Id", "PublicationId", "ReleaseDate", "Slug", "Title" },
                values: new object[] { new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5"), new Guid("2d94e5c8-a272-497c-bda0-c1f6b75155b0"), new DateTime(2019, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "2018", "2018" });

            migrationBuilder.InsertData(
                table: "Release",
                columns: new[] { "Id", "PublicationId", "ReleaseDate", "Slug", "Title" },
                values: new object[,]
                {
                    { new Guid("59258583-b075-47a2-bee4-5969e2d58873"), new Guid("99ce35fb-3fe2-48bb-9b73-23159df9d5ea"), new DateTime(2019, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "2018", "2018" },
                    { new Guid("463c8521-d9b4-4ccc-aee9-0666e39c8e47"), new Guid("5aea252e-fddc-42b3-a1da-47f12e523e70"), new DateTime(2019, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "2018", "2018" },
                    { new Guid("6ccc4416-7d22-46bf-a12a-56037831dc60"), new Guid("d5a01a5c-cd57-482f-8a19-803b266e1012"), new DateTime(2019, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "2018", "2018" },
                    { new Guid("0dafd89b-b754-44a8-b3f1-72baac0a108a"), new Guid("adb95888-64c7-4aa7-ba70-a9e535f8a30f"), new DateTime(2019, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "2018", "2018" },
                    { new Guid("dbaeb363-33fa-4928-870f-5054278e0c9a"), new Guid("90ecb3d3-bd05-4f84-a73e-1d153568b320"), new DateTime(2019, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "2018", "2018" },
                    { new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801"), new Guid("15659c96-a624-4457-846d-2ab5f3db6aec"), new DateTime(2019, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "2018", "2018" }
                });

            migrationBuilder.InsertData(
                table: "Subject",
                columns: new[] { "Id", "Name", "ReleaseId" },
                values: new object[,]
                {
                    { 18L, "New cases by age", new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5") },
                    { 19L, "Stock cases by age", new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5") },
                    { 20L, "New cases by establishment", new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5") },
                    { 21L, "Stock cases by establishment", new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5") },
                    { 22L, "Management information", new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5") }
                });

            migrationBuilder.InsertData(
                table: "Subject",
                columns: new[] { "Id", "Name", "ReleaseId" },
                values: new object[,]
                {
                    { 23L, "National achievement rates tables (NARTs)", new Guid("59258583-b075-47a2-bee4-5969e2d58873") },
                    { 24L, "Apprenticeship annual", new Guid("463c8521-d9b4-4ccc-aee9-0666e39c8e47") },
                    { 25L, "Further education and skills", new Guid("6ccc4416-7d22-46bf-a12a-56037831dc60") },
                    { 26L, "Level 2 and 3 National", new Guid("0dafd89b-b754-44a8-b3f1-72baac0a108a") },
                    { 27L, "Level 2 and 3 sf", new Guid("0dafd89b-b754-44a8-b3f1-72baac0a108a") },
                    { 28L, "Level 2 and 3 sf by Local authority", new Guid("0dafd89b-b754-44a8-b3f1-72baac0a108a") },
                    { 29L, "2016 test data", new Guid("dbaeb363-33fa-4928-870f-5054278e0c9a") },
                    { 30L, "Characteristic test data by Local authority", new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801") },
                    { 31L, "National characteristic test data", new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801") },
                    { 32L, "Subject tables S1 test data", new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801") },
                    { 33L, "Subject tables S3 test data", new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801") }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 18L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 19L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 20L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 21L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 22L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 23L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 24L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 25L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 26L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 27L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 28L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 29L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 30L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 31L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 32L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 33L);

            migrationBuilder.DeleteData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("0dafd89b-b754-44a8-b3f1-72baac0a108a"));

            migrationBuilder.DeleteData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("463c8521-d9b4-4ccc-aee9-0666e39c8e47"));

            migrationBuilder.DeleteData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("59258583-b075-47a2-bee4-5969e2d58873"));

            migrationBuilder.DeleteData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("6ccc4416-7d22-46bf-a12a-56037831dc60"));

            migrationBuilder.DeleteData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5"));

            migrationBuilder.DeleteData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801"));

            migrationBuilder.DeleteData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("dbaeb363-33fa-4928-870f-5054278e0c9a"));

            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("15659c96-a624-4457-846d-2ab5f3db6aec"));

            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("2d94e5c8-a272-497c-bda0-c1f6b75155b0"));

            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("5aea252e-fddc-42b3-a1da-47f12e523e70"));

            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("90ecb3d3-bd05-4f84-a73e-1d153568b320"));

            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("99ce35fb-3fe2-48bb-9b73-23159df9d5ea"));

            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("adb95888-64c7-4aa7-ba70-a9e535f8a30f"));

            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("d5a01a5c-cd57-482f-8a19-803b266e1012"));

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "Id",
                keyValue: new Guid("71444ff6-614f-405b-b6c7-f72077d42e34"));

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "Id",
                keyValue: new Guid("721048b9-8c06-4bad-8585-8789fa38a03b"));

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "Id",
                keyValue: new Guid("81fbb21d-3c49-46a2-8b43-0076974114f7"));

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "Id",
                keyValue: new Guid("9e4fa097-2999-4c4d-9ecd-0c4733fc71b4"));

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "Id",
                keyValue: new Guid("dfc908db-242a-4e3a-b6c6-e3f66cd152af"));

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "Id",
                keyValue: new Guid("f38469bd-a5f7-46b1-96bb-3b0a01e9e53f"));

            migrationBuilder.DeleteData(
                table: "Theme",
                keyColumn: "Id",
                keyValue: new Guid("9aa81762-e52c-40d4-8a90-f469977360a7"));

            migrationBuilder.DeleteData(
                table: "Theme",
                keyColumn: "Id",
                keyValue: new Guid("fe805471-17e9-4ac6-a555-c7d0ebec1b90"));
        }
    }
}
