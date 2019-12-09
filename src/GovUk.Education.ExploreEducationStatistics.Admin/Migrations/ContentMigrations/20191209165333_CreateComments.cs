using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class CreateComments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comment",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IContentBlockId = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Time = table.Column<DateTime>(nullable: false),
                    CommentText = table.Column<string>(nullable: true),
                    ResolvedBy = table.Column<string>(nullable: true),
                    ResolvedOn = table.Column<DateTime>(nullable: true),
                    State = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comment_ContentBlock_IContentBlockId",
                        column: x => x.IContentBlockId,
                        principalTable: "ContentBlock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Comment",
                columns: new[] { "Id", "CommentText", "IContentBlockId", "Name", "ResolvedBy", "ResolvedOn", "State", "Time" },
                values: new object[] { new Guid("514940e6-3b84-4e1b-aa5d-d1e5fa671e1b"), "Test Text", new Guid("a0b85d7d-a9bd-48b5-82c6-a119adc74ca2"), "A Test User", null, null, 0, new DateTime(2019, 12, 1, 15, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.CreateIndex(
                name: "IX_Comment_IContentBlockId",
                table: "Comment",
                column: "IContentBlockId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comment");
        }
    }
}
