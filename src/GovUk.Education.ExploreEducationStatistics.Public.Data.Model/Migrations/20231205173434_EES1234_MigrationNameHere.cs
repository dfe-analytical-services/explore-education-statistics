using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class EES1234_MigrationNameHere : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    PublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SupersedingDataSetId = table.Column<Guid>(type: "uuid", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSets_DataSets_SupersedingDataSetId",
                        column: x => x.SupersedingDataSetId,
                        principalTable: "DataSets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DataSetVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    CsvFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParquetFilename = table.Column<string>(type: "text", nullable: false),
                    VersionMajor = table.Column<int>(type: "integer", nullable: false),
                    VersionMinor = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Published = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MetaSummary = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSetVersions_DataSets_DataSetId",
                        column: x => x.DataSetId,
                        principalTable: "DataSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataSetChangeFilterOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetChangeFilterOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSetChangeFilterOptions_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataSetChangeFilters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetChangeFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSetChangeFilters_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataSetChangeIndicators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetChangeIndicators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSetChangeIndicators_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataSetChangeLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetChangeLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSetChangeLocations_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataSetChangeTimePeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetChangeTimePeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSetChangeTimePeriods_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataSetMeta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Filters = table.Column<string>(type: "jsonb", nullable: true),
                    Indicators = table.Column<string>(type: "jsonb", nullable: true),
                    Locations = table.Column<string>(type: "jsonb", nullable: true),
                    TimePeriods = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetMeta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSetMeta_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataSetChangeFilterOptions_DataSetVersionId",
                table: "DataSetChangeFilterOptions",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSetChangeFilters_DataSetVersionId",
                table: "DataSetChangeFilters",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSetChangeIndicators_DataSetVersionId",
                table: "DataSetChangeIndicators",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSetChangeLocations_DataSetVersionId",
                table: "DataSetChangeLocations",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSetChangeTimePeriods_DataSetVersionId",
                table: "DataSetChangeTimePeriods",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSetMeta_DataSetVersionId",
                table: "DataSetMeta",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSets_SupersedingDataSetId",
                table: "DataSets",
                column: "SupersedingDataSetId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSetVersions_DataSetId",
                table: "DataSetVersions",
                column: "DataSetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataSetChangeFilterOptions");

            migrationBuilder.DropTable(
                name: "DataSetChangeFilters");

            migrationBuilder.DropTable(
                name: "DataSetChangeIndicators");

            migrationBuilder.DropTable(
                name: "DataSetChangeLocations");

            migrationBuilder.DropTable(
                name: "DataSetChangeTimePeriods");

            migrationBuilder.DropTable(
                name: "DataSetMeta");

            migrationBuilder.DropTable(
                name: "DataSetVersions");

            migrationBuilder.DropTable(
                name: "DataSets");
        }
    }
}
