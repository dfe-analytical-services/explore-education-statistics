using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class AddTopicAndTheme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Release",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Release",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Theme",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Slug = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Theme", x => x.Id); });

            migrationBuilder.InsertData(
                table: "Theme",
                columns: new[] {"Id", "Name", "Slug"},
                values: new object[]
                {
                    new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"),
                    "Pupils and schools",
                    "pupils-and-schools"
                });

            migrationBuilder.InsertData(
                table: "Theme",
                columns: new[] {"Id", "Name", "Slug"},
                values: new object[]
                {
                    new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"),
                    "Children, early years and social care",
                    "children-and-early-years"
                });

            migrationBuilder.CreateTable(
                name: "Topic",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
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

            migrationBuilder.InsertData(
                table: "Topic",
                columns: new[] {"Id", "Name", "Slug", "ThemeId"},
                values: new object[]
                {
                    new Guid("67c249de-1cca-446e-8ccb-dcdac542f460"),
                    "Pupil absence",
                    "pupil-absence",
                    new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f")
                });
            
            migrationBuilder.InsertData(
                table: "Topic",
                columns: new[] {"Id", "Name", "Slug", "ThemeId"},
                values: new object[]
                {
                    new Guid("77941b7d-bbd6-4069-9107-565af89e2dec"),
                    "Exclusions",
                    "exclusions",
                    new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f")
                });
            
            migrationBuilder.InsertData(
                table: "Topic",
                columns: new[] {"Id", "Name", "Slug", "ThemeId"},
                values: new object[]
                {
                    new Guid("1a9636e4-29d5-4c90-8c07-f41db8dd019c"),
                    "School applications",
                    "school-applications",
                    new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f")
                });
            
            migrationBuilder.InsertData(
                table: "Topic",
                columns: new[] {"Id", "Name", "Slug", "ThemeId"},
                values: new object[]
                {
                    new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"),
                    "Early years foundation stage profile",
                    "early-years-foundation-stage-profile",
                    new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53")
                });
            
            migrationBuilder.CreateTable(
                name: "Publication",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
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
                table: "Publication",
                columns: new[] {"Id", "Name", "Slug", "TopicId"},
                values: new object[]
                {
                    new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    "Pupil absence data and statistics for schools in England",
                    "pupil-absence-in-schools-in-england",
                    new Guid("67c249de-1cca-446e-8ccb-dcdac542f460")
                });
            
            migrationBuilder.InsertData(
                table: "Publication",
                columns: new[] {"Id", "Name", "Slug", "TopicId"},
                values: new object[]
                {
                    new Guid("8345e27a-7a32-4b20-a056-309163bdf9c4"),
                    "Permanent and fixed-period exclusions in England",
                    "permanent-and-fixed-period-exclusions-in-england",
                    new Guid("77941b7d-bbd6-4069-9107-565af89e2dec")
                });
            
            migrationBuilder.InsertData(
                table: "Publication",
                columns: new[] {"Id", "Name", "Slug", "TopicId"},
                values: new object[]
                {
                    new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"),
                    "Secondary and primary schools applications and offers",
                    "secondary-and-primary-schools-applications-and-offers",
                    new Guid("1a9636e4-29d5-4c90-8c07-f41db8dd019c")
                });
            
            migrationBuilder.InsertData(
                table: "Publication",
                columns: new[] {"Id", "Name", "Slug", "TopicId"},
                values: new object[]
                {
                    new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f"),
                    "Early years foundation stage profile data",
                    "early-years-foundation-stage-profile-results",
                    new Guid("17b2e32c-ed2f-4896-852b-513cdf466769")
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
            
            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: 1,
                columns: new []{ "name", "slug"},
                values: new []
                {
                    "2016 to 2017",
                    "2016-17"
                });
            
            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: 2,
                columns: new []{ "name", "slug"},
                values: new []
                {
                    "2017 to 2018",
                    "2017-18"
                });
            
            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: 3,
                columns: new []{ "name", "slug"},
                values: new []
                {
                    "2016 to 2017",
                    "2016-17"
                });
            
            migrationBuilder.UpdateData(
                table: "Release",
                keyColumn: "Id",
                keyValue: 4,
                columns: new []{ "name", "slug"},
                values: new []
                {
                    "2018",
                    "2018"
                });
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

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Release");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Release");
        }
    }
}