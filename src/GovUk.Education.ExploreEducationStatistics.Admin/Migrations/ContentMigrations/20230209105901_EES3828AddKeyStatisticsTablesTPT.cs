using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES3828AddKeyStatisticsTablesTPT : Migration
    {
        private const string MigrationId = "20230209105901";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KeyStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReleaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Trend = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuidanceTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuidanceText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContentBlockIdTemp = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KeyStatistics_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KeyStatistics_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KeyStatistics_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "KeyStatisticsDataBlock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataBlockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyStatisticsDataBlock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KeyStatisticsDataBlock_ContentBlock_DataBlockId",
                        column: x => x.DataBlockId,
                        principalTable: "ContentBlock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KeyStatisticsDataBlock_KeyStatistics_Id",
                        column: x => x.Id,
                        principalTable: "KeyStatistics",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "KeyStatisticsText",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Statistic = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyStatisticsText", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KeyStatisticsText_KeyStatistics_Id",
                        column: x => x.Id,
                        principalTable: "KeyStatistics",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_KeyStatistics_CreatedById",
                table: "KeyStatistics",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_KeyStatistics_ReleaseId",
                table: "KeyStatistics",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_KeyStatistics_UpdatedById",
                table: "KeyStatistics",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_KeyStatisticsDataBlock_DataBlockId",
                table: "KeyStatisticsDataBlock",
                column: "DataBlockId");

            migrationBuilder.SqlFromFile(ContentMigrationsPath, $"{MigrationId}_GrantPermissionsForKeyStatTables.sql");
            migrationBuilder.SqlFromFile(ContentMigrationsPath, $"{MigrationId}_MigrateKeyStatDataBlockData.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeyStatisticsDataBlock");

            migrationBuilder.DropTable(
                name: "KeyStatisticsText");

            migrationBuilder.DropTable(
                name: "KeyStatistics");
        }
    }
}
