using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES4385_CreateMethodologyStatusTable : Migration
    {
        private const string MigrationId = "20230706081603";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MethodologyStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MethodologyVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InternalReleaseNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodologyStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MethodologyStatus_MethodologyVersions_MethodologyVersionId",
                        column: x => x.MethodologyVersionId,
                        principalTable: "MethodologyVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MethodologyStatus_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MethodologyStatus_CreatedById",
                table: "MethodologyStatus",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MethodologyStatus_MethodologyVersionId",
                table: "MethodologyStatus",
                column: "MethodologyVersionId");

            migrationBuilder.SqlFromFile(MigrationConstants.ContentMigrationsPath, $"{MigrationId}_MigrateMethodologyInternalReleaseNotes.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MethodologyStatus");
        }
    }
}
