using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees5105AddDataSetFileTables : Migration
    {
        private const string MigrationId = "20261013151651";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataSetFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetFiles", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "DataSetFileVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataSetFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RootPath = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Filename = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentLength = table.Column<long>(type: "bigint", nullable: false),
                    MetaFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MetaFilename = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaContentType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MetaContentLength = table.Column<long>(type: "bigint", nullable: true),
                    DataSetFileMeta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FilterHierarchies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReplacedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReplacingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetFileVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSetFileVersions_DataSetFileVersions_ReplacedById",
                        column: x => x.ReplacedById,
                        principalTable: "DataSetFileVersions",
                        principalColumn: "Id"
                    );
                    table.ForeignKey(
                        name: "FK_DataSetFileVersions_DataSetFileVersions_ReplacingId",
                        column: x => x.ReplacingId,
                        principalTable: "DataSetFileVersions",
                        principalColumn: "Id"
                    );
                    table.ForeignKey(
                        name: "FK_DataSetFileVersions_DataSetFiles_DataSetFileId",
                        column: x => x.DataSetFileId,
                        principalTable: "DataSetFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_DataSetFileVersions_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ReleaseVersionDataSetFileVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReleaseVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataSetFileVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    PublicApiCompatible = table.Column<bool>(type: "bit", nullable: true),
                    PublicApiDataSetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PublicApiDataSetVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FilterSequence = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IndicatorSequence = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Published = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseVersionDataSetFileVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleaseVersionDataSetFileVersions_DataSetFileVersions_DataSetFileVersionId",
                        column: x => x.DataSetFileVersionId,
                        principalTable: "DataSetFileVersions",
                        principalColumn: "Id"
                    );
                    table.ForeignKey(
                        name: "FK_ReleaseVersionDataSetFileVersions_ReleaseVersions_ReleaseVersionId",
                        column: x => x.ReleaseVersionId,
                        principalTable: "ReleaseVersions",
                        principalColumn: "Id"
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_DataSetFileVersions_CreatedById",
                table: "DataSetFileVersions",
                column: "CreatedById"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DataSetFileVersions_DataSetFileId_Version",
                table: "DataSetFileVersions",
                columns: new[] { "DataSetFileId", "Version" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_DataSetFileVersions_MetaFileId",
                table: "DataSetFileVersions",
                column: "MetaFileId",
                unique: true,
                filter: "[MetaFileId] IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DataSetFileVersions_ReplacedById",
                table: "DataSetFileVersions",
                column: "ReplacedById",
                unique: true,
                filter: "[ReplacedById] IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DataSetFileVersions_ReplacingId",
                table: "DataSetFileVersions",
                column: "ReplacingId",
                unique: true,
                filter: "[ReplacingId] IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DataSetFileVersions_SubjectId",
                table: "DataSetFileVersions",
                column: "SubjectId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseVersionDataSetFileVersions_DataSetFileVersionId",
                table: "ReleaseVersionDataSetFileVersions",
                column: "DataSetFileVersionId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseVersionDataSetFileVersions_ReleaseVersionId_DataSetFileVersionId",
                table: "ReleaseVersionDataSetFileVersions",
                columns: new[] { "ReleaseVersionId", "DataSetFileVersionId" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseVersionDataSetFileVersions_ReleaseVersionId_PublicApiDataSetId_PublicApiDataSetVersion",
                table: "ReleaseVersionDataSetFileVersions",
                columns: new[] { "ReleaseVersionId", "PublicApiDataSetId", "PublicApiDataSetVersion" },
                unique: true,
                filter: "[PublicApiDataSetId] IS NOT NULL AND [PublicApiDataSetVersion] IS NOT NULL"
            );

            migrationBuilder.SqlFromFile(
                MigrationConstants.ContentMigrationsPath,
                $"{MigrationId}_{nameof(Ees5105AddDataSetFileTables)}_Grants.sql"
            );

            migrationBuilder.SqlFromFile(
                MigrationConstants.ContentMigrationsPath,
                $"{MigrationId}_{nameof(Ees5105AddDataSetFileTables)}_Backfill.sql"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ReleaseVersionDataSetFileVersions");

            migrationBuilder.DropTable(name: "DataSetFileVersions");

            migrationBuilder.DropTable(name: "DataSetFiles");
        }
    }
}
