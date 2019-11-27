using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class AddJoinTableForReleaseAndContentBlock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReleaseContentBlocks",
                columns: table => new
                {
                    ReleaseId = table.Column<Guid>(nullable: false),
                    ContentBlockId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseContentBlocks", x => new { x.ReleaseId, x.ContentBlockId });
                    table.ForeignKey(
                        name: "FK_ReleaseContentBlocks_ContentBlock_ContentBlockId",
                        column: x => x.ContentBlockId,
                        principalTable: "ContentBlock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseContentBlocks_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ReleaseContentBlocks",
                columns: new[] { "ReleaseId", "ContentBlockId" },
                values: new object[,]
                {
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("9ccb0daf-91a1-4cb0-b3c1-2aed452338bc") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("3da30a08-9eeb-4a99-9872-796c3ea518fa") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("045a9585-688f-46fa-b3a9-9bdc237e0381") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("d0397918-1697-40d8-b649-bea3c63c7d3e") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("695de169-947f-4f66-8564-6392b6113dfc") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("17251e1c-e978-419c-98f5-963131c952f7") },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d") },
                    { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("5947759d-c6f3-451b-b353-a4da063f020a") },
                    { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("02a637e7-6cc7-44e5-8991-8982edfe49fc") },
                    { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("5d5f9b1f-8d0d-47d4-ba2b-ea97413d3117") },
                    { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseContentBlocks_ContentBlockId",
                table: "ReleaseContentBlocks",
                column: "ContentBlockId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReleaseContentBlocks");
        }
    }
}
