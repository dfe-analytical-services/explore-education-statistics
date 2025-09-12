using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class EES6489_AddEinTileGroupBlockAndEinTileTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "EinContentBlocks",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EinTiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    EinParentBlockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Statistic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Trend = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkText = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EinTiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EinTiles_EinContentBlocks_EinParentBlockId",
                        column: x => x.EinParentBlockId,
                        principalTable: "EinContentBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EinTiles_EinParentBlockId",
                table: "EinTiles",
                column: "EinParentBlockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EinTiles");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "EinContentBlocks");
        }
    }
}
