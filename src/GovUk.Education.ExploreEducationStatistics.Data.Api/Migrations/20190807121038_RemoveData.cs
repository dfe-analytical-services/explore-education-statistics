using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class RemoveData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove all Subjects and reset the Identity
            migrationBuilder.Sql("DELETE FROM Subject;");
            migrationBuilder.Sql("DBCC CHECKIDENT ('[Subject]', RESEED, 0);");

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
                keyValue: new Guid("47299b78-a4a6-4f7e-a86f-4713f4a0599a"));

            migrationBuilder.DeleteData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"));

            migrationBuilder.DeleteData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("59258583-b075-47a2-bee4-5969e2d58873"));

            migrationBuilder.DeleteData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"));

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
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"));

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
                keyValue: new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"));

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
                keyValue: new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"));

            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"));

            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"));

            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("cf0ec981-3583-42a5-b21b-3f2f32008f1b"));

            migrationBuilder.DeleteData(
                table: "Publication",
                keyColumn: "Id",
                keyValue: new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f"));

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "Id",
                keyValue: new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"));

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "Id",
                keyValue: new Guid("1a9636e4-29d5-4c90-8c07-f41db8dd019c"));

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "Id",
                keyValue: new Guid("1e763f55-bf09-4497-b838-7c5b054ba87b"));

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "Id",
                keyValue: new Guid("67c249de-1cca-446e-8ccb-dcdac542f460"));

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "Id",
                keyValue: new Guid("77941b7d-bbd6-4069-9107-565af89e2dec"));

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

            migrationBuilder.Sql("DELETE FROM Publication");

            migrationBuilder.Sql("DELETE FROM Topic");

            migrationBuilder.Sql("DELETE FROM Theme");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Theme",
                columns: new[] { "Id", "Slug", "Title" },
                values: new object[,]
                {
                    { new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"), "pupils-and-schools", "Pupils and schools" },
                    { new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "children-and-early-years", "Children, early years and social care" },
                    { new Guid("92c5df93-c4da-4629-ab25-51bd2920cdca"), "further-education", "Further education" },
                    { new Guid("74648781-85a9-4233-8be3-fe6f137165f4"), "outcomes-and-performance", "School and college outcomes and performance" }
                });

            migrationBuilder.InsertData(
                table: "Topic",
                columns: new[] { "Id", "Slug", "ThemeId", "Title" },
                values: new object[,]
                {
                    { new Guid("67c249de-1cca-446e-8ccb-dcdac542f460"), "pupil-absence", new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"), "Pupil absence" },
                    { new Guid("77941b7d-bbd6-4069-9107-565af89e2dec"), "exclusions", new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"), "Exclusions" },
                    { new Guid("1a9636e4-29d5-4c90-8c07-f41db8dd019c"), "school-applications", new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"), "School applications" },
                    { new Guid("85349b0a-19c7-4089-a56b-ad8dbe85449a"), "sen", new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"), "Special educational needs (SEN)" },
                    { new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"), "early-years-foundation-stage-profile", new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "Early years foundation stage profile" },
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
                    { new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "pupil-absence-in-schools-in-england", "Pupil absence in schools in England", new Guid("67c249de-1cca-446e-8ccb-dcdac542f460") },
                    { new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "permanent-and-fixed-period-exclusions-in-england", "Permanent and fixed-period exclusions in England", new Guid("77941b7d-bbd6-4069-9107-565af89e2dec") },
                    { new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"), "secondary-and-primary-schools-applications-and-offers", "Secondary and primary schools applications and offers", new Guid("1a9636e4-29d5-4c90-8c07-f41db8dd019c") },
                    { new Guid("88312cc0-fe1d-4ab5-81df-33fd708185cb"), "statements-of-sen-and-ehc-plans", "Statements of SEN and EHC plans", new Guid("85349b0a-19c7-4089-a56b-ad8dbe85449a") },
                    { new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f"), "early-years-foundation-stage-profile-results", "Early years foundation stage profile results", new Guid("17b2e32c-ed2f-4896-852b-513cdf466769") },
                    { new Guid("7a57d4c0-5233-4d46-8e27-748fbc365715"), "national-achievement-rates-tables", "National achievement rates tables", new Guid("dc7b7a89-e968-4a7e-af5f-bd7d19c346a5") },
                    { new Guid("cf0ec981-3583-42a5-b21b-3f2f32008f1b"), "apprenticeships-and-traineeships", "Apprenticeships and traineeships", new Guid("88d08425-fcfd-4c87-89da-70b2062a7367") },
                    { new Guid("13b81bcb-e8cd-4431-9807-ca588fd1d02a"), "further-education-and-skills", "Further education and skills", new Guid("88d08425-fcfd-4c87-89da-70b2062a7367") },
                    { new Guid("2e95f880-629c-417b-981f-0901e97776ff"), "Level 2 and 3 attainment by young people aged 19", "Level 2 and 3 attainment by young people aged 19", new Guid("85b5454b-3761-43b1-8e84-bd056a8efcd3") },
                    { new Guid("10370062-93b0-4dde-9097-5a56bf5b3064"), "national-curriculum-assessments-key-stage2", "National curriculum assessments at key stage 2", new Guid("eac38700-b968-4029-b8ac-0eb8e1356480") },
                    { new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"), "gcse-results-including-pupil-characteristics", "GCSE and equivalent results, including pupil characteristics", new Guid("1e763f55-bf09-4497-b838-7c5b054ba87b") }
                });

            migrationBuilder.InsertData(
                table: "Release",
                columns: new[] { "Id", "PublicationId", "ReleaseDate", "Slug", "Title" },
                values: new object[,]
                {
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), new DateTime(2018, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2016-17", "2016 to 2017" },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), new DateTime(2018, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "2016-17", "2016 to 2017" },
                    { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"), new DateTime(2019, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "2018", "2018" },
                    { new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5"), new Guid("88312cc0-fe1d-4ab5-81df-33fd708185cb"), new DateTime(2019, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "2018", "2018" },
                    { new Guid("47299b78-a4a6-4f7e-a86f-4713f4a0599a"), new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f"), new DateTime(2019, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017-18", "2017 to 2018" },
                    { new Guid("59258583-b075-47a2-bee4-5969e2d58873"), new Guid("7a57d4c0-5233-4d46-8e27-748fbc365715"), new DateTime(2019, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "2018", "2018" },
                    { new Guid("463c8521-d9b4-4ccc-aee9-0666e39c8e47"), new Guid("cf0ec981-3583-42a5-b21b-3f2f32008f1b"), new DateTime(2019, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "2018", "2018" },
                    { new Guid("6ccc4416-7d22-46bf-a12a-56037831dc60"), new Guid("13b81bcb-e8cd-4431-9807-ca588fd1d02a"), new DateTime(2019, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "2018", "2018" },
                    { new Guid("0dafd89b-b754-44a8-b3f1-72baac0a108a"), new Guid("2e95f880-629c-417b-981f-0901e97776ff"), new DateTime(2019, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "2018", "2018" },
                    { new Guid("dbaeb363-33fa-4928-870f-5054278e0c9a"), new Guid("10370062-93b0-4dde-9097-5a56bf5b3064"), new DateTime(2019, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "2018", "2018" },
                    { new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801"), new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"), new DateTime(2019, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "2018", "2018" }
                });

            migrationBuilder.InsertData(
                table: "Subject",
                columns: new[] { "Id", "Name", "ReleaseId" },
                values: new object[,]
                {
                    { 1L, "Absence by characteristic", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") },
                    { 31L, "National characteristic test data", new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801") },
                    { 30L, "Characteristic test data by Local authority", new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801") },
                    { 29L, "2016 test data", new Guid("dbaeb363-33fa-4928-870f-5054278e0c9a") },
                    { 28L, "Level 2 and 3 sf by Local authority", new Guid("0dafd89b-b754-44a8-b3f1-72baac0a108a") },
                    { 27L, "Level 2 and 3 sf", new Guid("0dafd89b-b754-44a8-b3f1-72baac0a108a") },
                    { 26L, "Level 2 and 3 National", new Guid("0dafd89b-b754-44a8-b3f1-72baac0a108a") },
                    { 25L, "Further education and skills", new Guid("6ccc4416-7d22-46bf-a12a-56037831dc60") },
                    { 24L, "Apprenticeship annual", new Guid("463c8521-d9b4-4ccc-aee9-0666e39c8e47") },
                    { 23L, "National achievement rates tables (NARTs)", new Guid("59258583-b075-47a2-bee4-5969e2d58873") },
                    { 10L, "APS GLD ELG underlying data 2013 - 2018", new Guid("47299b78-a4a6-4f7e-a86f-4713f4a0599a") },
                    { 9L, "Areas of learning underlying data 2013 - 2018", new Guid("47299b78-a4a6-4f7e-a86f-4713f4a0599a") },
                    { 8L, "ELG underlying data 2013 - 2018", new Guid("47299b78-a4a6-4f7e-a86f-4713f4a0599a") },
                    { 22L, "Management information", new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5") },
                    { 21L, "Stock cases by establishment", new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5") },
                    { 32L, "Subject tables S1 test data", new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801") },
                    { 20L, "New cases by establishment", new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5") },
                    { 18L, "New cases by age", new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5") },
                    { 17L, "Applications and offers by school phase", new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717") },
                    { 16L, "Total days missed due to fixed period exclusions", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") },
                    { 15L, "Number of fixed exclusions", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") },
                    { 14L, "Duration of fixed exclusions", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") },
                    { 13L, "Exclusions by reason", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") },
                    { 12L, "Exclusions by geographic level", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") },
                    { 11L, "Exclusions by characteristic", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") },
                    { 7L, "Absence rate percent bands", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") },
                    { 6L, "Absence number missing at least one session by reason", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") },
                    { 5L, "Absence in prus", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") },
                    { 4L, "Absence for four year olds", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") },
                    { 3L, "Absence by term", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") },
                    { 2L, "Absence by geographic level", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") },
                    { 19L, "Stock cases by age", new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5") },
                    { 33L, "Subject tables S3 test data", new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801") }
                });
        }
    }
}
