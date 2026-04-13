using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees6987RecreateEinPageTableAndOtherStuff : Migration
    {
        /// <inheritdoc />
        internal const string MigrationId = "20260423143431";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [EducationInNumbersPages];"); // ensure everything gets removed via cascade deletes

            migrationBuilder.DropForeignKey(
                name: "FK_EinContentSections_EducationInNumbersPages_EducationInNumbersPageId",
                table: "EinContentSections"
            );

            migrationBuilder.DropTable(name: "EducationInNumbersPages");

            migrationBuilder.RenameColumn(
                name: "EducationInNumbersPageId",
                table: "EinContentSections",
                newName: "EinPageVersionId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_EinContentSections_EducationInNumbersPageId",
                table: "EinContentSections",
                newName: "IX_EinContentSections_EinPageVersionId"
            );

            migrationBuilder.CreateTable(
                name: "EinPages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2047)", maxLength: 2047, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    LatestVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LatestPublishedVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EinPages", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "EinPageVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Published = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EinPageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EinPageVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EinPageVersions_EinPages_EinPageId",
                        column: x => x.EinPageId,
                        principalTable: "EinPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_EinPages_LatestPublishedVersionId",
                table: "EinPages",
                column: "LatestPublishedVersionId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_EinPages_LatestVersionId",
                table: "EinPages",
                column: "LatestVersionId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_EinPageVersions_EinPageId",
                table: "EinPageVersions",
                column: "EinPageId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_EinContentSections_EinPageVersions_EinPageVersionId",
                table: "EinContentSections",
                column: "EinPageVersionId",
                principalTable: "EinPageVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_EinPages_EinPageVersions_LatestPublishedVersionId",
                table: "EinPages",
                column: "LatestPublishedVersionId",
                principalTable: "EinPageVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_EinPages_EinPageVersions_LatestVersionId",
                table: "EinPages",
                column: "LatestVersionId",
                principalTable: "EinPageVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.SqlFromFile(
                MigrationConstants.ContentMigrationsPath,
                $"{MigrationId}_Ees6987AddEinRootPageAndVersion.sql"
            );

            migrationBuilder.Sql("GRANT SELECT ON dbo.EinPages TO [content]");
            migrationBuilder.Sql("GRANT SELECT ON dbo.EinPageVersions TO [content]");
            migrationBuilder.Sql("GRANT SELECT ON dbo.EinPages TO [publisher]");
            migrationBuilder.Sql("GRANT SELECT ON dbo.EinPageVersions TO [publisher]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
