using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class AddGoLiveContactList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("0d2ead36-3ebc-482f-a9c9-e17d746a0dd9"));

            migrationBuilder.UpdateData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("74f5aade-6d24-4a0b-be23-2ab4b4b2d191"),
                column: "ContactTelNo",
                value: "020 7783 8553");

            migrationBuilder.InsertData(
                table: "Contacts",
                columns: new[] { "Id", "ContactName", "ContactTelNo", "TeamEmail", "TeamName" },
                values: new object[,]
                {
                    { new Guid("d718aebb-bda6-4c3a-802d-8e9945b80997"), "Dave Bartholomew", "", "FE.OFFICIALSTATISTICS@education.gov.uk", "Further education statistical dissemination team" },
                    { new Guid("d5ed9ccc-5f7c-4c60-acbd-500461dbd680"), " Stephen Harris", "0161 600 1595", "destination.measures@education.gov.uk", "Destination measures statistics team" },
                    { new Guid("4de616ac-5124-4c28-b111-7d4bf63ee7b7"), "Andy Cooke", "07917 266106", "andy.cooke@education.gov.uk", "FE & Skills production team" },
                    { new Guid("1c696dea-8db7-4347-a12c-c9d081c34748"), "Emma Ibberson", "07824 082838", "TeachersAnalysisUnit.MAILBOX@education.gov.uk", "Teachers and teaching analysis unit" },
                    { new Guid("b40b586f-818f-4ef0-90d3-7ca4e65b0b00"), "", "01325 340 593", "AFB.BENCHMARKING@education.gov.uk", "Academies financial benchmarking team" },
                    { new Guid("2dc3af2d-574f-4ce5-8d37-0aa723b24d8d"), "Tony Clarke", "01325 340593", "finance.statistics@education.gov.uk", "Pupil and school finance data team" },
                    { new Guid("922ceecb-d0bf-4c2a-a166-928afecd4892"), "Selena Jackson", "020 7783 8599", "SCAP.PPP@education.gov.uk", "Pupil Place Planning team" },
                    { new Guid("4adb1382-faa1-4057-b9f5-6960d7f1465b"), "Anthony Clarke", "01325 340 593", "finance.statistics@education.gov.uk", "Data expert team" },
                    { new Guid("e4a5c74c-107a-4b56-965e-d8f0f68fe5cf"), "Chris Gray", "01325 340854", "CIN.Stats@education.gov.uk", "Children’s services statistics team - CIN" },
                    { new Guid("3653736f-3f11-4541-869f-0978c3bd026c"), "Dan Brown", "0114 274 2599", "CSWW.STATS@education.gov.uk", "Children’s services statistics team" },
                    { new Guid("c2ecb6e3-c539-4bf4-9376-51b9812a8447"), "Adina Huma", "0114 274 2313", "EY.AnalysisANDResearch@education.gov.uk", "Early years and childcare research and analysis" },
                    { new Guid("004ab2f0-606a-4ec2-8ab2-74bd5b931766"), "Jonathon Blackburn", "0161 600 1725", "EarlyYears.STATISTICS@education.gov.uk", "Early Years Analysis and Research" },
                    { new Guid("f4dd2a45-3538-47da-b40c-0cd270c185c6"), "Andy Cooke", "", "andy.cooke@education.gov.uk", "Further education statistical production team" },
                    { new Guid("ee8b0c92-b556-4670-904b-c265f0332a9e"), "Matthew Bridge", "07384 456648", "he.leo@education.gov.uk", "Higher education statistics team (LEO)" },
                    { new Guid("cd56905b-bbf0-42ed-8b07-4b750e2c0fae"), "David Collinge", "01325 340886", "", "Children looked-after statistics team" },
                    { new Guid("71f624fd-d8b8-4dc5-924c-4d768c9aa8fa"), "Helen Bray", "0370 000 2288", "PupilPopulation.PROJECTIONS@education.gov.uk", "Pupils and School Finance team" },
                    { new Guid("070f2584-c2a0-4fce-899d-bddfee7a803e"), "Matthew Rolfe", "", "FE.OFFICIALSTATISTICS@education.gov.uk", "Further education statistical dissemination team" },
                    { new Guid("308accc8-6b2e-4e26-b3e9-2437d5d1da23"), "Helen Bray", "0370 000 2288", "admissions.appeals@education.gov.uk", "Admission appeals statistics team" },
                    { new Guid("e4c2f394-4cb8-4d3b-969b-60f1e3550abf"), "Allan Burrage", "01325340986", "Attainment.STATISTICS@education.gov.uk", "Attainment statistics team" },
                    { new Guid("102497b9-ebd6-442a-b422-942ac391aaae"), "Tingting Shu", "0370 000 2288", "Attainment.STATISTICS@education.gov.uk", "Attainment statistics team" },
                    { new Guid("fb9030a8-8e5b-42a3-b621-1267f1ddf6f2"), "Glenn Goodman", "020 7654 6408", "Attainment.STATISTICS@education.gov.uk", "Attainment statistics team" },
                    { new Guid("2ed7e605-b7e5-4c02-ae8f-f01fe9261c37"), "Chris Noble", "01325 340 688", "EarlyYears.STATISTICS@education.gov.uk", "Early years statistics team" },
                    { new Guid("6a0e8cba-dbb6-4d79-a672-7114b82f8347"), "Jeanette D'Costa", "020 7783 8699", "ittstatistics.publications@education.gov.uk", "Initial Teacher Training Statistics Publications" },
                    { new Guid("667f03a6-fd2f-41a2-a28d-3721662633c3"), "John Simes", "0370 000 2288", "HE.statistics@education.gov.uk", "Widening participation statistics" },
                    { new Guid("f65786b2-fb7e-4ee9-84dc-78fddd057252"), "Gemma Coleman", "020 7783 8239", "primary.attainment@education.gov.uk", "Primary attainment statistics team" },
                    { new Guid("94b3f9c3-6169-4e85-bf26-08d69bcb1a4a"), "Anastasia Ioannou", "0370 000 2288", "Schools.STATISTICS@education.gov.uk", "Infrastructure statistics team" },
                    { new Guid("7536aef1-03c6-44cc-971f-9e0d4deebb82"), "Heather Brown", "0114 274 2755", "schoolworkforce.statistics@education.gov.uk", "Teachers and teaching statistics team" },
                    { new Guid("f47cb404-6bf1-4f69-b223-c9a6a2c15082"), "Suzanne Wallace", "020 7654 6191", "post16.statistics@education.gov.uk", "Post-16 statistics team" },
                    { new Guid("6a95bda0-a823-4faa-9a13-7e65f5b6c121"), "Ann Claytor", "0114 274 2515", "post16.statistics@education.gov.uk", "Post-16 statistics team" },
                    { new Guid("8d1469b6-f029-4dcc-92fa-bda8ab2c69cd"), "Nick Treece", "0114 2742728", "FE.OUTCOMESDATA@education.gov.uk", "Further education outcomes statistics" },
                    { new Guid("367a6e6f-48c5-4f1a-9580-f44852bc8e7a"), "Alana Afflick", "07469 413 560", "Marking.STA@education.gov.uk", "Standards and Testing Agency" },
                    { new Guid("0e7d435f-2177-4063-bad2-4b3dcdb17ea8"), "Adam Hatton", "020 7340 8364", "Academies.DATA@education.gov.uk", "Academies and school organisation team" },
                    { new Guid("2e38c23a-564b-4228-bffe-fa6f88b7bec4"), "", "", "InternationalEvidence.STATISTICS@education.gov.uk", "International evidence and statistics team" },
                    { new Guid("ee490e40-201a-4b25-bc52-76c15de72344"), "Sally Marshall", "0114 274 2317", "post16.statistics@education.gov.uk", "Post-16 statistics team" },
                    { new Guid("9f67f7ec-e6e6-439e-829e-fb52e634c5f5"), "Justin Ushie", "01325340817", "cla.stats@education.gov.uk", "Looked-after children statistics team" }
                });

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("14953fda-02ff-45ed-9573-3a7a0ad8cb10"),
                column: "ContactId",
                value: new Guid("d246c696-4b3a-4aeb-842c-c1318ee334e8"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("6c388293-d027-4f74-8d74-29a42e02231c"),
                column: "ContactId",
                value: new Guid("d246c696-4b3a-4aeb-842c-c1318ee334e8"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("86af24dc-67c4-47f0-a849-e94c7a1cfe9b"),
                column: "ContactId",
                value: new Guid("d246c696-4b3a-4aeb-842c-c1318ee334e8"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("88312cc0-fe1d-4ab5-81df-33fd708185cb"),
                column: "ContactId",
                value: new Guid("0b63e6c7-5a9d-4c48-b30f-f0729e0644c0"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("c8756008-ed50-4632-9b96-01b5ca002a43"),
                column: "ContactId",
                value: new Guid("18c9a473-465d-4b8a-b2cf-b24fd3b9c094"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("f657afb4-8f4a-427d-a683-15f11a2aefb5"),
                column: "ContactId",
                value: new Guid("0b63e6c7-5a9d-4c48-b30f-f0729e0644c0"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("060c5376-35d8-420b-8266-517a9339b7bc"),
                column: "ContactId",
                value: new Guid("004ab2f0-606a-4ec2-8ab2-74bd5b931766"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("0ce6a6c6-5451-4967-8dd4-2f4fa8131982"),
                column: "ContactId",
                value: new Guid("2ed7e605-b7e5-4c02-ae8f-f01fe9261c37"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("123461ab-50be-45d9-8523-c5241a2c9c5b"),
                column: "ContactId",
                value: new Guid("308accc8-6b2e-4e26-b3e9-2437d5d1da23"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("1b2fb05c-eb2c-486b-80be-ebd772eda4f1"),
                column: "ContactId",
                value: new Guid("102497b9-ebd6-442a-b422-942ac391aaae"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("1d0e4263-3d70-433e-bd95-f29754db5888"),
                column: "ContactId",
                value: new Guid("e4c2f394-4cb8-4d3b-969b-60f1e3550abf"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2434335f-f8e1-41fb-8d6e-4a11bc62b14a"),
                column: "ContactId",
                value: new Guid("f65786b2-fb7e-4ee9-84dc-78fddd057252"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("263e10d2-b9c3-4e90-a6aa-b52b86de1f5f"),
                column: "ContactId",
                value: new Guid("102497b9-ebd6-442a-b422-942ac391aaae"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("28aabfd4-a3fb-45e1-bb34-21ca3b7d1aec"),
                column: "ContactId",
                value: new Guid("fb9030a8-8e5b-42a3-b621-1267f1ddf6f2"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2e510281-ca8c-41bf-bbe0-fd15fcc81aae"),
                column: "ContactId",
                value: new Guid("ee490e40-201a-4b25-bc52-76c15de72344"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2e95f880-629c-417b-981f-0901e97776ff"),
                column: "ContactId",
                value: new Guid("f47cb404-6bf1-4f69-b223-c9a6a2c15082"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2ffbc8d3-eb53-4c4b-a6fb-219a5b95ebc8"),
                column: "ContactId",
                value: new Guid("2e38c23a-564b-4228-bffe-fa6f88b7bec4"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("3260801d-601a-48c6-93b7-cf51680323d1"),
                column: "ContactId",
                value: new Guid("9f67f7ec-e6e6-439e-829e-fb52e634c5f5"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("3f3a66ec-5777-42ee-b427-8102a14ce0c5"),
                column: "ContactId",
                value: new Guid("102497b9-ebd6-442a-b422-942ac391aaae"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("441a13f6-877c-4f18-828f-119dbd401a5b"),
                column: "ContactId",
                value: new Guid("f65786b2-fb7e-4ee9-84dc-78fddd057252"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("4d29c28c-efd1-4245-a80c-b55c6a50e3f7"),
                column: "ContactId",
                value: new Guid("ee8b0c92-b556-4670-904b-c265f0332a9e"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("657b1484-0369-4a0e-873a-367b79a48c35"),
                column: "ContactId",
                value: new Guid("4de616ac-5124-4c28-b111-7d4bf63ee7b7"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("75568912-25ba-499a-8a96-6161b54994db"),
                column: "ContactId",
                value: new Guid("4de616ac-5124-4c28-b111-7d4bf63ee7b7"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("79a08466-dace-4ff0-94b6-59c5528c9262"),
                column: "ContactId",
                value: new Guid("c2ecb6e3-c539-4bf4-9376-51b9812a8447"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("7a57d4c0-5233-4d46-8e27-748fbc365715"),
                column: "ContactId",
                value: new Guid("f4dd2a45-3538-47da-b40c-0cd270c185c6"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("7ecea655-7b22-4832-b697-26e86769399a"),
                column: "ContactId",
                value: new Guid("367a6e6f-48c5-4f1a-9580-f44852bc8e7a"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("89869bba-0c00-40f7-b7d6-e28cb904ad37"),
                column: "ContactId",
                value: new Guid("e4a5c74c-107a-4b56-965e-d8f0f68fe5cf"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("8a92c6a5-8110-4c9c-87b1-e15f1c80c66a"),
                column: "ContactId",
                value: new Guid("d5ed9ccc-5f7c-4c60-acbd-500461dbd680"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("8ab47806-e36f-4226-9988-1efe23156872"),
                column: "ContactId",
                value: new Guid("b40b586f-818f-4ef0-90d3-7ca4e65b0b00"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("8b12776b-3d36-4475-8115-00974d7de1d0"),
                column: "ContactId",
                value: new Guid("8d1469b6-f029-4dcc-92fa-bda8ab2c69cd"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("94d16c6e-1e5f-48d5-8195-8ea770f1b0d4"),
                column: "ContactId",
                value: new Guid("4adb1382-faa1-4057-b9f5-6960d7f1465b"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("9cc08298-7370-499f-919a-7d203ba21415"),
                column: "ContactId",
                value: new Guid("6a0e8cba-dbb6-4d79-a672-7114b82f8347"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("9e7e9d5c-b761-43a4-9685-4892392200b7"),
                column: "ContactId",
                value: new Guid("e4c2f394-4cb8-4d3b-969b-60f1e3550abf"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a0eb117e-44a8-4732-adf1-8fbc890cbb62"),
                column: "ContactId",
                value: new Guid("ee490e40-201a-4b25-bc52-76c15de72344"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                column: "ContactId",
                value: new Guid("94b3f9c3-6169-4e85-bf26-08d69bcb1a4a"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("aa545525-9ffe-496c-a5b3-974ace56746e"),
                column: "ContactId",
                value: new Guid("71f624fd-d8b8-4dc5-924c-4d768c9aa8fa"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("b318967f-2931-472a-93f2-fbed1e181e6a"),
                column: "ContactId",
                value: new Guid("7536aef1-03c6-44cc-971f-9e0d4deebb82"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bddcd4b8-db0d-446c-b6e9-03d4230c6927"),
                column: "ContactId",
                value: new Guid("f65786b2-fb7e-4ee9-84dc-78fddd057252"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"),
                column: "ContactId",
                value: new Guid("fb9030a8-8e5b-42a3-b621-1267f1ddf6f2"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("c28f7aca-f1e8-4916-8ce3-fc177b140695"),
                column: "ContactId",
                value: new Guid("667f03a6-fd2f-41a2-a28d-3721662633c3"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("cf0ec981-3583-42a5-b21b-3f2f32008f1b"),
                column: "ContactId",
                value: new Guid("070f2584-c2a0-4fce-899d-bddfee7a803e"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d0b47c96-d7de-4d80-9ff7-3bff135d2636"),
                column: "ContactId",
                value: new Guid("1c696dea-8db7-4347-a12c-c9d081c34748"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d24783b6-24a7-4ef3-8304-fd07eeedff92"),
                column: "ContactId",
                value: new Guid("070f2584-c2a0-4fce-899d-bddfee7a803e"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d34978d5-0317-46bc-9258-13412270ac4d"),
                column: "ContactId",
                value: new Guid("6a0e8cba-dbb6-4d79-a672-7114b82f8347"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d7bd5d9d-dc65-4b1d-99b1-4d815b7369a3"),
                column: "ContactId",
                value: new Guid("9f67f7ec-e6e6-439e-829e-fb52e634c5f5"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d8baee79-3c88-45f4-b12a-07b91e9b5c11"),
                column: "ContactId",
                value: new Guid("3653736f-3f11-4541-869f-0978c3bd026c"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("dcb8b32b-4e50-4fe2-a539-58f9b6b3a366"),
                column: "ContactId",
                value: new Guid("2dc3af2d-574f-4ce5-8d37-0aa723b24d8d"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("eab51107-4ef0-4926-8f8b-c8bd7f5a21d5"),
                column: "ContactId",
                value: new Guid("0e7d435f-2177-4063-bad2-4b3dcdb17ea8"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("f00a784b-52e8-475b-b8ee-dbe730382ba8"),
                columns: new[] { "ContactId", "Title" },
                values: new object[] { new Guid("4de616ac-5124-4c28-b111-7d4bf63ee7b7"), "FE choices employer satisfaction survey" });

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("f51895df-c682-45e6-b23e-3138ddbfdaeb"),
                column: "ContactId",
                value: new Guid("cd56905b-bbf0-42ed-8b07-4b750e2c0fae"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("fa591a15-ae37-41b5-98f6-4ce06e5225f4"),
                column: "ContactId",
                value: new Guid("922ceecb-d0bf-4c2a-a166-928afecd4892"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f"),
                column: "ContactId",
                value: new Guid("2ed7e605-b7e5-4c02-ae8f-f01fe9261c37"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("6a95bda0-a823-4faa-9a13-7e65f5b6c121"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("d718aebb-bda6-4c3a-802d-8e9945b80997"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("004ab2f0-606a-4ec2-8ab2-74bd5b931766"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("070f2584-c2a0-4fce-899d-bddfee7a803e"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("0e7d435f-2177-4063-bad2-4b3dcdb17ea8"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("102497b9-ebd6-442a-b422-942ac391aaae"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("1c696dea-8db7-4347-a12c-c9d081c34748"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("2dc3af2d-574f-4ce5-8d37-0aa723b24d8d"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("2e38c23a-564b-4228-bffe-fa6f88b7bec4"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("2ed7e605-b7e5-4c02-ae8f-f01fe9261c37"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("308accc8-6b2e-4e26-b3e9-2437d5d1da23"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("3653736f-3f11-4541-869f-0978c3bd026c"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("367a6e6f-48c5-4f1a-9580-f44852bc8e7a"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("4adb1382-faa1-4057-b9f5-6960d7f1465b"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("4de616ac-5124-4c28-b111-7d4bf63ee7b7"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("667f03a6-fd2f-41a2-a28d-3721662633c3"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("6a0e8cba-dbb6-4d79-a672-7114b82f8347"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("71f624fd-d8b8-4dc5-924c-4d768c9aa8fa"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("7536aef1-03c6-44cc-971f-9e0d4deebb82"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("8d1469b6-f029-4dcc-92fa-bda8ab2c69cd"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("922ceecb-d0bf-4c2a-a166-928afecd4892"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("94b3f9c3-6169-4e85-bf26-08d69bcb1a4a"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("9f67f7ec-e6e6-439e-829e-fb52e634c5f5"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("b40b586f-818f-4ef0-90d3-7ca4e65b0b00"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("c2ecb6e3-c539-4bf4-9376-51b9812a8447"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("cd56905b-bbf0-42ed-8b07-4b750e2c0fae"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("d5ed9ccc-5f7c-4c60-acbd-500461dbd680"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("e4a5c74c-107a-4b56-965e-d8f0f68fe5cf"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("e4c2f394-4cb8-4d3b-969b-60f1e3550abf"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("ee490e40-201a-4b25-bc52-76c15de72344"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("ee8b0c92-b556-4670-904b-c265f0332a9e"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("f47cb404-6bf1-4f69-b223-c9a6a2c15082"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("f4dd2a45-3538-47da-b40c-0cd270c185c6"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("f65786b2-fb7e-4ee9-84dc-78fddd057252"));

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("fb9030a8-8e5b-42a3-b621-1267f1ddf6f2"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("060c5376-35d8-420b-8266-517a9339b7bc"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("0ce6a6c6-5451-4967-8dd4-2f4fa8131982"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("123461ab-50be-45d9-8523-c5241a2c9c5b"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("1b2fb05c-eb2c-486b-80be-ebd772eda4f1"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("1d0e4263-3d70-433e-bd95-f29754db5888"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2434335f-f8e1-41fb-8d6e-4a11bc62b14a"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("263e10d2-b9c3-4e90-a6aa-b52b86de1f5f"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("28aabfd4-a3fb-45e1-bb34-21ca3b7d1aec"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2e510281-ca8c-41bf-bbe0-fd15fcc81aae"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2e95f880-629c-417b-981f-0901e97776ff"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2ffbc8d3-eb53-4c4b-a6fb-219a5b95ebc8"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("3260801d-601a-48c6-93b7-cf51680323d1"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("3f3a66ec-5777-42ee-b427-8102a14ce0c5"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("441a13f6-877c-4f18-828f-119dbd401a5b"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("4d29c28c-efd1-4245-a80c-b55c6a50e3f7"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("657b1484-0369-4a0e-873a-367b79a48c35"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("75568912-25ba-499a-8a96-6161b54994db"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("79a08466-dace-4ff0-94b6-59c5528c9262"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("7a57d4c0-5233-4d46-8e27-748fbc365715"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("7ecea655-7b22-4832-b697-26e86769399a"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("89869bba-0c00-40f7-b7d6-e28cb904ad37"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("8a92c6a5-8110-4c9c-87b1-e15f1c80c66a"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("8ab47806-e36f-4226-9988-1efe23156872"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("8b12776b-3d36-4475-8115-00974d7de1d0"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("94d16c6e-1e5f-48d5-8195-8ea770f1b0d4"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("9cc08298-7370-499f-919a-7d203ba21415"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("9e7e9d5c-b761-43a4-9685-4892392200b7"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a0eb117e-44a8-4732-adf1-8fbc890cbb62"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("aa545525-9ffe-496c-a5b3-974ace56746e"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("b318967f-2931-472a-93f2-fbed1e181e6a"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bddcd4b8-db0d-446c-b6e9-03d4230c6927"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("c28f7aca-f1e8-4916-8ce3-fc177b140695"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("cf0ec981-3583-42a5-b21b-3f2f32008f1b"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d0b47c96-d7de-4d80-9ff7-3bff135d2636"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d24783b6-24a7-4ef3-8304-fd07eeedff92"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d34978d5-0317-46bc-9258-13412270ac4d"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d7bd5d9d-dc65-4b1d-99b1-4d815b7369a3"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d8baee79-3c88-45f4-b12a-07b91e9b5c11"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("dcb8b32b-4e50-4fe2-a539-58f9b6b3a366"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("eab51107-4ef0-4926-8f8b-c8bd7f5a21d5"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("f00a784b-52e8-475b-b8ee-dbe730382ba8"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("f51895df-c682-45e6-b23e-3138ddbfdaeb"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("fa591a15-ae37-41b5-98f6-4ce06e5225f4"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("74f5aade-6d24-4a0b-be23-2ab4b4b2d191"),
                column: "ContactTelNo",
                value: "02077838553");

            migrationBuilder.InsertData(
                table: "Contacts",
                columns: new[] { "Id", "ContactName", "ContactTelNo", "TeamEmail", "TeamName" },
                values: new object[] { new Guid("0d2ead36-3ebc-482f-a9c9-e17d746a0dd9"), "Justin Ushie", "01325340817", "cla.stats@education.gov.uk", "Looked-after children statistics team" });

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("060c5376-35d8-420b-8266-517a9339b7bc"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("0ce6a6c6-5451-4967-8dd4-2f4fa8131982"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("123461ab-50be-45d9-8523-c5241a2c9c5b"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("14953fda-02ff-45ed-9573-3a7a0ad8cb10"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("1b2fb05c-eb2c-486b-80be-ebd772eda4f1"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("1d0e4263-3d70-433e-bd95-f29754db5888"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2434335f-f8e1-41fb-8d6e-4a11bc62b14a"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("263e10d2-b9c3-4e90-a6aa-b52b86de1f5f"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("28aabfd4-a3fb-45e1-bb34-21ca3b7d1aec"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2e510281-ca8c-41bf-bbe0-fd15fcc81aae"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2e95f880-629c-417b-981f-0901e97776ff"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2ffbc8d3-eb53-4c4b-a6fb-219a5b95ebc8"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("3260801d-601a-48c6-93b7-cf51680323d1"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("3f3a66ec-5777-42ee-b427-8102a14ce0c5"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("441a13f6-877c-4f18-828f-119dbd401a5b"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("4d29c28c-efd1-4245-a80c-b55c6a50e3f7"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("657b1484-0369-4a0e-873a-367b79a48c35"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("6c388293-d027-4f74-8d74-29a42e02231c"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("75568912-25ba-499a-8a96-6161b54994db"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("79a08466-dace-4ff0-94b6-59c5528c9262"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("7a57d4c0-5233-4d46-8e27-748fbc365715"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("7ecea655-7b22-4832-b697-26e86769399a"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("86af24dc-67c4-47f0-a849-e94c7a1cfe9b"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("88312cc0-fe1d-4ab5-81df-33fd708185cb"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("89869bba-0c00-40f7-b7d6-e28cb904ad37"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("8a92c6a5-8110-4c9c-87b1-e15f1c80c66a"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("8ab47806-e36f-4226-9988-1efe23156872"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("8b12776b-3d36-4475-8115-00974d7de1d0"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("94d16c6e-1e5f-48d5-8195-8ea770f1b0d4"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("9cc08298-7370-499f-919a-7d203ba21415"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("9e7e9d5c-b761-43a4-9685-4892392200b7"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a0eb117e-44a8-4732-adf1-8fbc890cbb62"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("aa545525-9ffe-496c-a5b3-974ace56746e"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("b318967f-2931-472a-93f2-fbed1e181e6a"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bddcd4b8-db0d-446c-b6e9-03d4230c6927"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("c28f7aca-f1e8-4916-8ce3-fc177b140695"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("c8756008-ed50-4632-9b96-01b5ca002a43"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("cf0ec981-3583-42a5-b21b-3f2f32008f1b"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d0b47c96-d7de-4d80-9ff7-3bff135d2636"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d24783b6-24a7-4ef3-8304-fd07eeedff92"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d34978d5-0317-46bc-9258-13412270ac4d"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d7bd5d9d-dc65-4b1d-99b1-4d815b7369a3"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d8baee79-3c88-45f4-b12a-07b91e9b5c11"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("dcb8b32b-4e50-4fe2-a539-58f9b6b3a366"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("eab51107-4ef0-4926-8f8b-c8bd7f5a21d5"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("f00a784b-52e8-475b-b8ee-dbe730382ba8"),
                columns: new[] { "ContactId", "Title" },
                values: new object[] { null, "FE chioces employer satisfaction survey" });

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("f51895df-c682-45e6-b23e-3138ddbfdaeb"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("f657afb4-8f4a-427d-a683-15f11a2aefb5"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("fa591a15-ae37-41b5-98f6-4ce06e5225f4"),
                column: "ContactId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f"),
                column: "ContactId",
                value: null);
        }
    }
}
