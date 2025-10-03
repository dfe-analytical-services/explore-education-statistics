using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class EES6373_AddEducationInNumbersContentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EinContentSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Heading = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    EducationInNumbersPageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EinContentSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EinContentSections_EducationInNumbersPages_EducationInNumbersPageId",
                        column: x => x.EducationInNumbersPageId,
                        principalTable: "EducationInNumbersPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "EinContentBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    EinContentSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EinContentBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EinContentBlocks_EinContentSections_EinContentSectionId",
                        column: x => x.EinContentSectionId,
                        principalTable: "EinContentSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_EinContentBlocks_EinContentSectionId",
                table: "EinContentBlocks",
                column: "EinContentSectionId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_EinContentSections_EducationInNumbersPageId",
                table: "EinContentSections",
                column: "EducationInNumbersPageId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "EinContentBlocks");

            migrationBuilder.DropTable(name: "EinContentSections");
        }
    }
}
