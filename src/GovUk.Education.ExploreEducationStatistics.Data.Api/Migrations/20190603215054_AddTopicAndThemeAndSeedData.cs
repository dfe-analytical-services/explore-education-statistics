using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class AddTopicAndThemeAndSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Release",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Release",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Theme",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Slug = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Theme", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Topic",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Slug = table.Column<string>(nullable: true),
                    ThemeId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topic", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Topic_Theme_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Theme",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Publication",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Slug = table.Column<string>(nullable: true),
                    TopicId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Publication_Topic_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Theme",
                columns: new[] { "Id", "Slug", "Title" },
                values: new object[] { new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"), "pupils-and-schools", "Pupils and schools" });

            migrationBuilder.InsertData(
                table: "Theme",
                columns: new[] { "Id", "Slug", "Title" },
                values: new object[] { new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "children-and-early-years", "Children, early years and social care" });

            migrationBuilder.InsertData(
                table: "Topic",
                columns: new[] { "Id", "Slug", "ThemeId", "Title" },
                values: new object[,]
                {
                    { new Guid("67c249de-1cca-446e-8ccb-dcdac542f460"), "pupil-absence", new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"), "Pupil absence" },
                    { new Guid("77941b7d-bbd6-4069-9107-565af89e2dec"), "exclusions", new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"), "Exclusions" },
                    { new Guid("1a9636e4-29d5-4c90-8c07-f41db8dd019c"), "school-applications", new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"), "School applications" },
                    { new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"), "early-years-foundation-stage-profile", new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "Early years foundation stage profile" }
                });

            migrationBuilder.InsertData(
                table: "Publication",
                columns: new[] { "Id", "Slug", "Title", "TopicId" },
                values: new object[,]
                {
                    { new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "pupil-absence-in-schools-in-england", "Pupil absence data and statistics for schools in England", new Guid("67c249de-1cca-446e-8ccb-dcdac542f460") },
                    { new Guid("8345e27a-7a32-4b20-a056-309163bdf9c4"), "permanent-and-fixed-period-exclusions-in-england", "Permanent and fixed-period exclusions in England", new Guid("77941b7d-bbd6-4069-9107-565af89e2dec") },
                    { new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"), "secondary-and-primary-schools-applications-and-offers", "Secondary and primary schools applications and offers", new Guid("1a9636e4-29d5-4c90-8c07-f41db8dd019c") },
                    { new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f"), "early-years-foundation-stage-profile-results", "Early years foundation stage profile data", new Guid("17b2e32c-ed2f-4896-852b-513cdf466769") }
                });

            migrationBuilder.InsertData(
                table: "Release",
                columns: new[] { "Id", "PublicationId", "ReleaseDate", "Slug", "Title" },
                values: new object[,]
                {
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), new DateTime(2018, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2016-17", "2016 to 2017" },
                    { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), new Guid("8345e27a-7a32-4b20-a056-309163bdf9c4"), new DateTime(2018, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "2016-17", "2016 to 2017" },
                    { new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"), new DateTime(2019, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "2018", "2018" },
                    { new Guid("47299b78-a4a6-4f7e-a86f-4713f4a0599a"), new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f"), new DateTime(2019, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017-18", "2017 to 2018" }
                });

            migrationBuilder.InsertData(
                table: "Subject",
                columns: new[] { "Id", "Name", "ReleaseId" },
                values: new object[,]
                {
                    { 1L, "Absence by characteristic", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") },
                    { 8L, "ELG underlying data 2013 - 2018", new Guid("47299b78-a4a6-4f7e-a86f-4713f4a0599a") },
                    { 17L, "Applications and offers by school phase", new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717") },
                    { 16L, "Total days missed due to fixed period exclusions", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") },
                    { 15L, "Number of fixed exclusions", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") },
                    { 14L, "Duration of fixed exclusions", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") },
                    { 13L, "Exclusions by reason", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") },
                    { 9L, "Areas of learning underlying data 2013 - 2018", new Guid("47299b78-a4a6-4f7e-a86f-4713f4a0599a") },
                    { 12L, "Exclusions by geographic level", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") },
                    { 7L, "Absence rate percent bands", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") },
                    { 6L, "Absence number missing at least one session by reason", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") },
                    { 5L, "Absence in prus", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") },
                    { 4L, "Absence for four year olds", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") },
                    { 3L, "Absence by term", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") },
                    { 2L, "Absence by geographic level", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") },
                    { 11L, "Exclusions by characteristic", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") },
                    { 10L, "APS GLD ELG underlying data 2013 - 2018", new Guid("47299b78-a4a6-4f7e-a86f-4713f4a0599a") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Publication_TopicId",
                table: "Publication",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_Topic_ThemeId",
                table: "Topic",
                column: "ThemeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Release_Publication_PublicationId",
                table: "Release",
                column: "PublicationId",
                principalTable: "Publication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Release_Publication_PublicationId",
                table: "Release");

            migrationBuilder.DropTable(
                name: "Publication");

            migrationBuilder.DropTable(
                name: "Topic");

            migrationBuilder.DropTable(
                name: "Theme");

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 12L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 13L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 14L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 15L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 16L);

            migrationBuilder.DeleteData(
                table: "Subject",
                keyColumn: "Id",
                keyValue: 17L);

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
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"));

            migrationBuilder.DeleteData(
                table: "Release",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"));

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Release");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Release");
        }
    }
}
