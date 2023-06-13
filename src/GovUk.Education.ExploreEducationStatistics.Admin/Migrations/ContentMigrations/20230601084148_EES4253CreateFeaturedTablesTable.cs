using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{

    public partial class EES4253CreateFeaturedTablesTable : Migration
    {
        private const string MigrationId = "20230601084148";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeaturedTables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    DataBlockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReleaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeaturedTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeaturedTables_ContentBlock_DataBlockId",
                        column: x => x.DataBlockId,
                        principalTable: "ContentBlock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeaturedTables_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeaturedTables_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeaturedTables_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedTables_CreatedById",
                table: "FeaturedTables",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedTables_DataBlockId",
                table: "FeaturedTables",
                column: "DataBlockId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedTables_ReleaseId",
                table: "FeaturedTables",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedTables_UpdatedById",
                table: "FeaturedTables",
                column: "UpdatedById");

            migrationBuilder.Sql("GRANT SELECT ON [dbo].[FeaturedTables] TO [publisher]");
            migrationBuilder.Sql("GRANT SELECT ON [dbo].[FeaturedTables] TO [data]");
            migrationBuilder.Sql("GRANT SELECT ON [dbo].[FeaturedTables] TO [content]");

            migrationBuilder.SqlFromFile(MigrationConstants.ContentMigrationsPath, $"{MigrationId}_MigrateFeaturedTableData.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeaturedTables");
        }
    }
}
