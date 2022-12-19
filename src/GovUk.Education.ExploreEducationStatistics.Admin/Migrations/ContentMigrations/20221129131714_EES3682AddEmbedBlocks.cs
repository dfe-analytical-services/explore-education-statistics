using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES3682AddEmbedBlocks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EmbedBlockId",
                table: "ContentBlock",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmbedBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmbedBlocks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentBlock_EmbedBlockId",
                table: "ContentBlock",
                column: "EmbedBlockId",
                unique: true,
                filter: "[EmbedBlockId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentBlock_EmbedBlocks_EmbedBlockId",
                table: "ContentBlock",
                column: "EmbedBlockId",
                principalTable: "EmbedBlocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("GRANT SELECT ON dbo.EmbedBlocks TO [publisher]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("REVOKE SELECT ON dbo.EmbedBlocks TO [publisher]");

            migrationBuilder.DropForeignKey(
                name: "FK_ContentBlock_EmbedBlocks_EmbedBlockId",
                table: "ContentBlock");

            migrationBuilder.DropTable(
                name: "EmbedBlocks");

            migrationBuilder.DropIndex(
                name: "IX_ContentBlock_EmbedBlockId",
                table: "ContentBlock");

            migrationBuilder.DropColumn(
                name: "EmbedBlockId",
                table: "ContentBlock");
        }
    }
}
