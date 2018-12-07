using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.DataDissemination.Content.Api.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    ThemeId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Topics_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Publications",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    TopicId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Publications_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Themes",
                columns: new[] { "Id", "Title" },
                values: new object[] { new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "Schools" });

            migrationBuilder.InsertData(
                table: "Themes",
                columns: new[] { "Id", "Title" },
                values: new object[] { new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"), "Social Care" });

            migrationBuilder.InsertData(
                table: "Themes",
                columns: new[] { "Id", "Title" },
                values: new object[] { new Guid("bc08839f-2970-4f34-af2d-29608a48082f"), "16+" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "ThemeId", "Title" },
                values: new object[] { new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "Absence and Exclusions" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "ThemeId", "Title" },
                values: new object[] { new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"), new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "School and Pupil Numbers" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "ThemeId", "Title" },
                values: new object[] { new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5"), new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "Capacity Admissions" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "ThemeId", "Title" },
                values: new object[] { new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"), new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "Results" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "ThemeId", "Title" },
                values: new object[] { new Guid("66ff5e67-36cf-4210-9ad2-632baeb4eca7"), new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "School Finance" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "ThemeId", "Title" },
                values: new object[] { new Guid("d5288137-e703-43a1-b634-d50fc9785cb9"), new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "Teacher Numbers" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "ThemeId", "Title" },
                values: new object[] { new Guid("0b920c62-ff67-4cf1-89ec-0c74a364e6b4"), new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"), "Number of Children" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "ThemeId", "Title" },
                values: new object[] { new Guid("3bef5b2b-76a1-4be1-83b1-a3269245c610"), new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"), "Vulnerable Children" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "ThemeId", "Title" },
                values: new object[] { new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"), new Guid("bc08839f-2970-4f34-af2d-29608a48082f"), "Further Education" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "ThemeId", "Title" },
                values: new object[] { new Guid("4c658598-450b-4493-b972-8812acd154a7"), new Guid("bc08839f-2970-4f34-af2d-29608a48082f"), "Higher Education" });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "Title", "TopicId" },
                values: new object[] { new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "Pupil absence in schools in England", new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852") });

            migrationBuilder.CreateIndex(
                name: "IX_Publications_TopicId",
                table: "Publications",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_ThemeId",
                table: "Topics",
                column: "ThemeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Publications");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropTable(
                name: "Themes");
        }
    }
}
