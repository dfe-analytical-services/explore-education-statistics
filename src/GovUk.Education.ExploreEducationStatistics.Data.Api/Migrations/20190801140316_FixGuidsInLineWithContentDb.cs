using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class FixGuidsInLineWithContentDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.InsertData(
                table: "Theme",
                columns: new[] { "Id", "Slug", "Title" },
                values: new object[] { new Guid("92c5df93-c4da-4629-ab25-51bd2920cdca"), "further-education", "Further education" });

            migrationBuilder.InsertData(
                table: "Theme",
                columns: new[] { "Id", "Slug", "Title" },
                values: new object[] { new Guid("74648781-85a9-4233-8be3-fe6f137165f4"), "outcomes-and-performance", "School and college outcomes and performance" });

            migrationBuilder.InsertData(
                table: "Topic",
                columns: new[] { "Id", "Slug", "ThemeId", "Title" },
                values: new object[] { new Guid("85349b0a-19c7-4089-a56b-ad8dbe85449a"), "sen", new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"), "Special educational needs (SEN)" });

            migrationBuilder.InsertData(
                table: "Publication",
                columns: new[] { "Id", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("88312cc0-fe1d-4ab5-81df-33fd708185cb"), "statements-of-sen-and-ehc-plans", "Statements of SEN and EHC plans", new Guid("85349b0a-19c7-4089-a56b-ad8dbe85449a") });

            migrationBuilder.InsertData(
                table: "Topic",
                columns: new[] { "Id", "Slug", "ThemeId", "Title" },
                values: new object[,]
                {
                    { new Guid("dc7b7a89-e968-4a7e-af5f-bd7d19c346a5"), "national-achievement-rates-tables", new Guid("92c5df93-c4da-4629-ab25-51bd2920cdca"), "National achievement rates tables" },
                    { new Guid("88d08425-fcfd-4c87-89da-70b2062a7367"), "further-education-and-skills", new Guid("92c5df93-c4da-4629-ab25-51bd2920cdca"), "Further education and skills" },
                    { new Guid("85b5454b-3761-43b1-8e84-bd056a8efcd3"), "sixteen-to-nineteen-attainment", new Guid("74648781-85a9-4233-8be3-fe6f137165f4"), "16 to 19 attainment" },
                    { new Guid("eac38700-b968-4029-b8ac-0eb8e1356480"), "key-stage-two", new Guid("74648781-85a9-4233-8be3-fe6f137165f4"), "Key stage 2" },
                    { new Guid("1e763f55-bf09-4497-b838-7c5b054ba87b"), "key-stage-four", new Guid("74648781-85a9-4233-8be3-fe6f137165f4"), "GCSEs (key stage 4)" }
                });

            migrationBuilder.InsertData(
                table: "Publication",
                columns: new[] { "Id", "Slug", "Title", "TopicId" },
                values: new object[,]
                {
                    { new Guid("7a57d4c0-5233-4d46-8e27-748fbc365715"), "national-achievement-rates-tables", "National achievement rates tables", new Guid("dc7b7a89-e968-4a7e-af5f-bd7d19c346a5") },
                    { new Guid("cf0ec981-3583-42a5-b21b-3f2f32008f1b"), "apprenticeships-and-traineeships", "Apprenticeships and traineeships", new Guid("88d08425-fcfd-4c87-89da-70b2062a7367") },
                    { new Guid("13b81bcb-e8cd-4431-9807-ca588fd1d02a"), "further-education-and-skills", "Further education and skills", new Guid("88d08425-fcfd-4c87-89da-70b2062a7367") },
                    { new Guid("2e95f880-629c-417b-981f-0901e97776ff"), "Level 2 and 3 attainment by young people aged 19", "Level 2 and 3 attainment by young people aged 19", new Guid("85b5454b-3761-43b1-8e84-bd056a8efcd3") },
                    { new Guid("10370062-93b0-4dde-9097-5a56bf5b3064"), "national-curriculum-assessments-key-stage2", "National curriculum assessments at key stage 2", new Guid("eac38700-b968-4029-b8ac-0eb8e1356480") },
                    { new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"), "gcse-results-including-pupil-characteristics", "GCSE and equivalent results, including pupil characteristics", new Guid("1e763f55-bf09-4497-b838-7c5b054ba87b") }
                });

            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5"),
                column: "PublicationId",
                value: new Guid("88312cc0-fe1d-4ab5-81df-33fd708185cb"));

            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("0dafd89b-b754-44a8-b3f1-72baac0a108a"),
                column: "PublicationId",
                value: new Guid("2e95f880-629c-417b-981f-0901e97776ff"));

            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("463c8521-d9b4-4ccc-aee9-0666e39c8e47"),
                column: "PublicationId",
                value: new Guid("cf0ec981-3583-42a5-b21b-3f2f32008f1b"));

            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("59258583-b075-47a2-bee4-5969e2d58873"),
                column: "PublicationId",
                value: new Guid("7a57d4c0-5233-4d46-8e27-748fbc365715"));

            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("6ccc4416-7d22-46bf-a12a-56037831dc60"),
                column: "PublicationId",
                value: new Guid("13b81bcb-e8cd-4431-9807-ca588fd1d02a"));

            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801"),
                column: "PublicationId",
                value: new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"));

            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("dbaeb363-33fa-4928-870f-5054278e0c9a"),
                column: "PublicationId",
                value: new Guid("10370062-93b0-4dde-9097-5a56bf5b3064"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("10370062-93b0-4dde-9097-5a56bf5b3064"));

            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("13b81bcb-e8cd-4431-9807-ca588fd1d02a"));

            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("2e95f880-629c-417b-981f-0901e97776ff"));

            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("7a57d4c0-5233-4d46-8e27-748fbc365715"));

            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("88312cc0-fe1d-4ab5-81df-33fd708185cb"));

            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"));

            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("cf0ec981-3583-42a5-b21b-3f2f32008f1b"));

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "Id",
                keyValue: new Guid("1e763f55-bf09-4497-b838-7c5b054ba87b"));

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "Id",
                keyValue: new Guid("85349b0a-19c7-4089-a56b-ad8dbe85449a"));

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "Id",
                keyValue: new Guid("85b5454b-3761-43b1-8e84-bd056a8efcd3"));

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "Id",
                keyValue: new Guid("88d08425-fcfd-4c87-89da-70b2062a7367"));

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "Id",
                keyValue: new Guid("dc7b7a89-e968-4a7e-af5f-bd7d19c346a5"));

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "Id",
                keyValue: new Guid("eac38700-b968-4029-b8ac-0eb8e1356480"));

            migrationBuilder.DeleteData(
                table: "Theme",
                keyColumn: "Id",
                keyValue: new Guid("74648781-85a9-4233-8be3-fe6f137165f4"));

            migrationBuilder.DeleteData(
                table: "Theme",
                keyColumn: "Id",
                keyValue: new Guid("92c5df93-c4da-4629-ab25-51bd2920cdca"));

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

            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5"),
                column: "PublicationId",
                value: new Guid("2d94e5c8-a272-497c-bda0-c1f6b75155b0"));

            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("0dafd89b-b754-44a8-b3f1-72baac0a108a"),
                column: "PublicationId",
                value: new Guid("adb95888-64c7-4aa7-ba70-a9e535f8a30f"));

            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("463c8521-d9b4-4ccc-aee9-0666e39c8e47"),
                column: "PublicationId",
                value: new Guid("5aea252e-fddc-42b3-a1da-47f12e523e70"));

            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("59258583-b075-47a2-bee4-5969e2d58873"),
                column: "PublicationId",
                value: new Guid("99ce35fb-3fe2-48bb-9b73-23159df9d5ea"));

            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("6ccc4416-7d22-46bf-a12a-56037831dc60"),
                column: "PublicationId",
                value: new Guid("d5a01a5c-cd57-482f-8a19-803b266e1012"));

            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801"),
                column: "PublicationId",
                value: new Guid("15659c96-a624-4457-846d-2ab5f3db6aec"));

            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("dbaeb363-33fa-4928-870f-5054278e0c9a"),
                column: "PublicationId",
                value: new Guid("90ecb3d3-bd05-4f84-a73e-1d153568b320"));
        }
    }
}
