using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class AddJoinTableForReleaseAndContentSection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentSections_Releases_ReleaseId",
                table: "ContentSections");

            migrationBuilder.DropForeignKey(
                name: "FK_Releases_ContentBlock_KeyStatisticsId",
                table: "Releases");

            migrationBuilder.DropIndex(
                name: "IX_Releases_KeyStatisticsId",
                table: "Releases");

            migrationBuilder.DropIndex(
                name: "IX_ContentSections_ReleaseId",
                table: "ContentSections");

            migrationBuilder.DropColumn(
                name: "KeyStatisticsId",
                table: "Releases");

            migrationBuilder.DropColumn(
                name: "ReleaseId",
                table: "ContentSections");

            migrationBuilder.CreateTable(
                name: "ReleaseContentSections",
                columns: table => new
                {
                    ReleaseId = table.Column<Guid>(nullable: false),
                    ContentSectionId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseContentSections", x => new { x.ReleaseId, x.ContentSectionId });
                    table.ForeignKey(
                        name: "FK_ReleaseContentSections_ContentSections_ContentSectionId",
                        column: x => x.ContentSectionId,
                        principalTable: "ContentSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseContentSections_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ReleaseContentSections",
                columns: new[] { "ReleaseId", "ContentSectionId" },
                values: new object[,]
                {
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("601aadcc-be7d-4d3e-9154-c9eb64144692") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("c0241ab7-f40a-4755-bc69-365eba8114a3") },
                    { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("de8f8547-cbae-4d52-88ec-d78d0ad836ae") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("991a436a-9c7a-418b-ab06-60f2610b4bc6") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("24c6e9a3-1415-4ca5-9f21-b6b51cb7ba94") },
                    { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("93ef0486-479f-4013-8012-a66ed01f1880") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("8965ef44-5ad7-4ab0-a142-78453d6f40af") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("6f493eee-443a-4403-9069-fef82e2f5788") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("fbf99442-3b72-46bc-836d-8866c552c53d") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("6898538c-3f8d-488d-9e50-12ca7a9fd70c") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("08b204a2-0eeb-4797-9e0b-a1274e7f6a38") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("7b779d79-6caa-43fd-84ba-b8efd219b3c8") },
                    { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("8abdae8f-4119-41ac-8efd-2229b7ea31da") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("f599c2e2-f215-423a-beab-c5c6a0c2e5a9") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("4f30b382-ce28-4a3e-801a-ce76004f5eb4") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("60f8c7ca-faff-4f0d-937d-17fe376461cf") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("68f8b290-4b7c-4cac-b0d9-0263609c341b") },
                    { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("def347bd-0b29-405f-a11f-cd03c853a6ed") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("5600ca55-6800-418a-94a5-2f3c3310304e") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("015d0cdd-6630-4b57-9ef3-7341fc3d573e") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("50e7ca4c-e6c7-4ccd-afc1-93ee4298f358") },
                    { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("6bfa9b19-25d6-4d45-8008-9447db541795") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("3960ab94-0fad-442c-8aaa-6233eff3bc32") },
                    { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("c1f17b4e-f576-40bc-80e1-63767998d080") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("6ed87fd1-81a5-46dc-8841-4598bdae7fee") },
                    { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("c3eb66d0-ce13-4e68-861d-98bb914d0814") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("b7a968ab-eb49-4100-b133-3d9d94f23d60") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("68e3028c-1291-42b3-9e7c-9be285dac9a1") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("d5d604af-6b63-4a51-b106-0c09b8dbedfa") },
                    { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("b87f2e62-e3e7-4492-9d68-18df8dc29041") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("7981db34-afdb-4f84-99e8-bfd43e58f16d") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("5708d443-7669-47d8-b6a3-6ad851090710") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("30d74065-66b8-4843-9761-4578519e1394") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("e8a813ce-c68a-417b-af31-91db19377b10") },
                    { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("39c298e9-6c5f-47be-85cb-6e49b1b1931f") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseContentSections_ContentSectionId",
                table: "ReleaseContentSections",
                column: "ContentSectionId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReleaseContentSections");

            migrationBuilder.AddColumn<Guid>(
                name: "KeyStatisticsId",
                table: "Releases",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReleaseId",
                table: "ContentSections",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("015d0cdd-6630-4b57-9ef3-7341fc3d573e"),
                column: "ReleaseId",
                value: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("08b204a2-0eeb-4797-9e0b-a1274e7f6a38"),
                column: "ReleaseId",
                value: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("24c6e9a3-1415-4ca5-9f21-b6b51cb7ba94"),
                column: "ReleaseId",
                value: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("3960ab94-0fad-442c-8aaa-6233eff3bc32"),
                column: "ReleaseId",
                value: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("4f30b382-ce28-4a3e-801a-ce76004f5eb4"),
                column: "ReleaseId",
                value: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("50e7ca4c-e6c7-4ccd-afc1-93ee4298f358"),
                column: "ReleaseId",
                value: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("5600ca55-6800-418a-94a5-2f3c3310304e"),
                column: "ReleaseId",
                value: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("5708d443-7669-47d8-b6a3-6ad851090710"),
                column: "ReleaseId",
                value: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("601aadcc-be7d-4d3e-9154-c9eb64144692"),
                column: "ReleaseId",
                value: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("60f8c7ca-faff-4f0d-937d-17fe376461cf"),
                column: "ReleaseId",
                value: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("6898538c-3f8d-488d-9e50-12ca7a9fd70c"),
                column: "ReleaseId",
                value: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("68e3028c-1291-42b3-9e7c-9be285dac9a1"),
                column: "ReleaseId",
                value: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("68f8b290-4b7c-4cac-b0d9-0263609c341b"),
                column: "ReleaseId",
                value: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("6bfa9b19-25d6-4d45-8008-9447db541795"),
                column: "ReleaseId",
                value: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("6ed87fd1-81a5-46dc-8841-4598bdae7fee"),
                column: "ReleaseId",
                value: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("6f493eee-443a-4403-9069-fef82e2f5788"),
                column: "ReleaseId",
                value: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("7981db34-afdb-4f84-99e8-bfd43e58f16d"),
                column: "ReleaseId",
                value: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("7b779d79-6caa-43fd-84ba-b8efd219b3c8"),
                column: "ReleaseId",
                value: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("8965ef44-5ad7-4ab0-a142-78453d6f40af"),
                column: "ReleaseId",
                value: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("8abdae8f-4119-41ac-8efd-2229b7ea31da"),
                column: "ReleaseId",
                value: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("93ef0486-479f-4013-8012-a66ed01f1880"),
                column: "ReleaseId",
                value: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("991a436a-9c7a-418b-ab06-60f2610b4bc6"),
                column: "ReleaseId",
                value: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("b7a968ab-eb49-4100-b133-3d9d94f23d60"),
                column: "ReleaseId",
                value: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("b87f2e62-e3e7-4492-9d68-18df8dc29041"),
                column: "ReleaseId",
                value: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("c0241ab7-f40a-4755-bc69-365eba8114a3"),
                column: "ReleaseId",
                value: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("c1f17b4e-f576-40bc-80e1-63767998d080"),
                column: "ReleaseId",
                value: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("c3eb66d0-ce13-4e68-861d-98bb914d0814"),
                column: "ReleaseId",
                value: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("d5d604af-6b63-4a51-b106-0c09b8dbedfa"),
                column: "ReleaseId",
                value: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("de8f8547-cbae-4d52-88ec-d78d0ad836ae"),
                column: "ReleaseId",
                value: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("def347bd-0b29-405f-a11f-cd03c853a6ed"),
                column: "ReleaseId",
                value: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("f599c2e2-f215-423a-beab-c5c6a0c2e5a9"),
                column: "ReleaseId",
                value: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"));

            migrationBuilder.UpdateData(
                table: "ContentSections",
                keyColumn: "Id",
                keyValue: new Guid("fbf99442-3b72-46bc-836d-8866c552c53d"),
                column: "ReleaseId",
                value: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"));

            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("8ab41234-cc9d-4b3d-a42c-c9fce7762719"),
                column: "Content",
                value: "[{\"Id\":\"d82b2a2c-b117-4f96-b812-80de5304ae21\",\"Order\":1,\"Heading\":\"1. Overview of applications and offers statistics\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"ff7b34e3-58ba-4578-849a-e5044fc14b8d\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"f0814433-92d4-4ce5-b63b-2f2cb1b6f48a\",\"Order\":2,\"Heading\":\"2. The admissions process\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"d7f310d7-e917-47e2-9e05-065c8bcab891\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"1d7a492b-3e59-4624-9a2a-076635d1f780\",\"Order\":3,\"Heading\":\"3. Methodology\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"a89226ef-1d6a-48ba-a795-0dbc334a9198\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"f129939b-803f-461b-8838-e7a3d8c6eca2\",\"Order\":4,\"Heading\":\"4. Contacts\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"16898320-cf69-45c4-8dbb-2486901759b1\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"}]");

            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("c8c911e3-39c1-452b-801f-25bb79d1deb7"),
                columns: new[] { "Annexes", "Content" },
                values: new object[] { "[{\"Id\":\"2bb1ce6d-8b54-4a77-bf7d-466c5f7f6bc3\",\"Order\":1,\"Heading\":\"Annex A - Calculations\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"c2d4d345-59b6-443b-bcb0-67c1f1dd9732\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"01e9feb8-8ca0-4d98-8a17-78672e4641a7\",\"Order\":2,\"Heading\":\"Annex B - Exclusion by reason codes\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"e4e4f98b-cbeb-451f-bd17-8e2d572b83f4\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"39576875-4a54-4028-bdb0-fecc67041f82\",\"Order\":3,\"Heading\":\"Annex C - Links to pupil exclusions statistics and data\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"94246f85-e43a-4b6c-97a0-b045701dc077\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"e3bfcc04-7d91-45b7-b0ee-19713de4b433\",\"Order\":4,\"Heading\":\"Annex D - Standard breakdowns\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"b05ae1f7-b2d1-4ae3-9db6-28fb0edf98ae\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"}]", "[{\"Id\":\"bceaafc1-9548-4a03-98d5-d3476c8b9d99\",\"Order\":1,\"Heading\":\"1. Overview of exclusion statistics\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"9a034a5f-7cdb-4895-b205-864e7f834ebf\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"66b15928-46c6-48d5-90e6-12cf354b4e04\",\"Order\":2,\"Heading\":\"2. National Statistics badging\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"f745893e-68a6-4813-ba8b-35d44c0935aa\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"863f2b02-67b1-41bd-b1c9-f998f4581297\",\"Order\":3,\"Heading\":\"3. Methodology\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"4c88cbdd-e0e2-4019-a656-31e4b97d19d5\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"fc66f72e-0176-4c75-b15f-2f35c7329563\",\"Order\":4,\"Heading\":\"4. Data collection\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"085ba061-918b-4e6e-9a02-3a8b12671587\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"0c44636a-9a31-4e05-8db7-331ed5eae366\",\"Order\":5,\"Heading\":\"5. Data processing\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"29f138eb-070e-41fe-95c6-e271cdf4eaf4\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"69df08b6-dcda-449e-828e-5666c8e6d533\",\"Order\":6,\"Heading\":\"6. Data quality\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"8263cdc1-a2e8-4db6-a581-44043a6add64\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"fa315759-a51b-4860-8ae5-7b9505873108\",\"Order\":7,\"Heading\":\"7. Contacts\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"e1eb03d9-25e4-4247-b3de-49805cce7889\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"}]" });

            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("caa8e56f-41d2-4129-a5c3-53b051134bd7"),
                columns: new[] { "Annexes", "Content" },
                values: new object[] { "[{\"Id\":\"0522bb29-1e0d-455a-88ef-5887f76fb069\",\"Order\":1,\"Heading\":\"Annex A - Calculations\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"8b90b3b2-f63d-4499-91aa-41ccae74e1c7\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"f1aac714-665d-436e-a488-1ca409d618bf\",\"Order\":2,\"Heading\":\"Annex B - School attendance codes\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"47f3e500-ec9f-4a00-96f8-c488f76b06e6\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"0b888133-215a-4b28-8c24-e0ee9a32df6e\",\"Order\":3,\"Heading\":\"Annex C - Links to pupil absence national statistics and data\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"a00a7765-aa81-43f2-afe1-fead7f070291\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"4c4c71e2-24e1-4b57-8a23-ce54fae9b329\",\"Order\":4,\"Heading\":\"Annex D - Standard breakdowns\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"7cc516d4-fc79-4e22-b35b-a042d5b14d35\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"97a138bf-4ebb-4b17-86ab-ed78584608e3\",\"Order\":5,\"Heading\":\"Annex E - Timeline\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"8ddc6877-acd2-479d-a86f-1139c1bd429f\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"dc00e749-0893-47f7-8440-5a4da47ceed7\",\"Order\":6,\"Heading\":\"Annex F - Absence rates over time\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"0fd71cfd-cfdd-42c5-86e7-9e311beee646\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"}]", "[{\"Id\":\"5a7fd947-d131-475d-afcd-11ab2b1ece67\",\"Order\":1,\"Heading\":\"1. Overview of absence statistics\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"4d5ae97d-fa1c-4a09-a0a3-b28307fcfb09\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"dabb7562-0433-42fc-96e4-64a68f399dac\",\"Order\":2,\"Heading\":\"2. National Statistics badging\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"6bf20dd4-a7d6-4bc6-a13a-9f574935c9af\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"50b5031a-93e4-4756-843e-21f88f52ba68\",\"Order\":3,\"Heading\":\"3. Methodology\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"63a318d9-05fa-40eb-9808-b825a6deb54a\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"e4ca520f-b609-4abb-a38c-c2d610a18e9f\",\"Order\":4,\"Heading\":\"4. Data collection\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"7714efb9-cc82-4895-ba27-bf5464541e38\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"da91d355-b878-4135-a0a9-fb538c601246\",\"Order\":5,\"Heading\":\"5. Data processing\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"6f81ab70-5730-4cf1-a513-669f5c4bef09\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"8df45966-5444-4487-be49-763c5009eea6\",\"Order\":6,\"Heading\":\"6. Data quality\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"a40d6c9e-fe61-48c0-b907-9757148beb0d\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"},{\"Id\":\"bf6870de-07d3-4e65-a877-373a63dbcc5d\",\"Order\":7,\"Heading\":\"7. Contacts\",\"Caption\":\"\",\"Content\":[{\"Body\":\"\",\"Type\":\"HtmlBlock\",\"Id\":\"f620a229-21b7-4c6e-afd4-e9feb111f09a\",\"Order\":0}],\"Release\":null,\"ReleaseId\":\"00000000-0000-0000-0000-000000000000\"}]" });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "KeyStatisticsId",
                value: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"));

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                column: "KeyStatisticsId",
                value: new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e"));

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "KeyStatisticsId",
                value: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"));

            migrationBuilder.CreateIndex(
                name: "IX_Releases_KeyStatisticsId",
                table: "Releases",
                column: "KeyStatisticsId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentSections_ReleaseId",
                table: "ContentSections",
                column: "ReleaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentSections_Releases_ReleaseId",
                table: "ContentSections",
                column: "ReleaseId",
                principalTable: "Releases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Releases_ContentBlock_KeyStatisticsId",
                table: "Releases",
                column: "KeyStatisticsId",
                principalTable: "ContentBlock",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}