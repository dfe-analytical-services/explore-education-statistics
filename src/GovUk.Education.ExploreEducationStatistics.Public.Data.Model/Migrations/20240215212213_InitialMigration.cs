using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChangeSetFilterOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeSetFilterOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeSetFilters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeSetFilters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeSetIndicators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeSetIndicators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeSetLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeSetLocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeSetTimePeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeSetTimePeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    PublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SupersedingDataSetId = table.Column<Guid>(type: "uuid", nullable: true),
                    LatestVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Published = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Unpublished = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    Status = table.Column<string>(type: "text", nullable: false),
                    CsvFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParquetFilename = table.Column<string>(type: "text", nullable: false),
                    VersionMajor = table.Column<int>(type: "integer", nullable: false),
                    VersionMinor = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    TotalResults = table.Column<long>(type: "bigint", nullable: false),
                    Published = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Unpublished = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                name: "FilterMetas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    Hint = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Options = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterMetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilterMetas_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeographicLevelMetas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Options = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeographicLevelMetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeographicLevelMetas_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndicatorMetas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Options = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicatorMetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndicatorMetas_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationMetas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LocalAuthorityOptions = table.Column<string>(type: "jsonb", nullable: true),
                    Options = table.Column<string>(type: "jsonb", nullable: true),
                    ProviderOptions = table.Column<string>(type: "jsonb", nullable: true),
                    RscRegionOptions = table.Column<string>(type: "jsonb", nullable: true),
                    SchoolOptions = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationMetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationMetas_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimePeriodMetas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Options = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimePeriodMetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimePeriodMetas_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChangeSetFilterOptions_DataSetVersionId",
                table: "ChangeSetFilterOptions",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeSetFilters_DataSetVersionId",
                table: "ChangeSetFilters",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeSetIndicators_DataSetVersionId",
                table: "ChangeSetIndicators",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeSetLocations_DataSetVersionId",
                table: "ChangeSetLocations",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeSetTimePeriods_DataSetVersionId",
                table: "ChangeSetTimePeriods",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSets_LatestVersionId",
                table: "DataSets",
                column: "LatestVersionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataSets_SupersedingDataSetId",
                table: "DataSets",
                column: "SupersedingDataSetId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSetVersions_DataSetId",
                table: "DataSetVersions",
                column: "DataSetId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterMetas_DataSetVersionId_PublicId",
                table: "FilterMetas",
                columns: new[] { "DataSetVersionId", "PublicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeographicLevelMetas_DataSetVersionId",
                table: "GeographicLevelMetas",
                column: "DataSetVersionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorMetas_DataSetVersionId",
                table: "IndicatorMetas",
                column: "DataSetVersionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationMetas_DataSetVersionId",
                table: "LocationMetas",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationMetas_Level_DataSetVersionId",
                table: "LocationMetas",
                columns: new[] { "Level", "DataSetVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationMetas_Type_DataSetVersionId",
                table: "LocationMetas",
                columns: new[] { "Type", "DataSetVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_TimePeriodMetas_DataSetVersionId",
                table: "TimePeriodMetas",
                column: "DataSetVersionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeSetFilterOptions_DataSetVersions_DataSetVersionId",
                table: "ChangeSetFilterOptions",
                column: "DataSetVersionId",
                principalTable: "DataSetVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeSetFilters_DataSetVersions_DataSetVersionId",
                table: "ChangeSetFilters",
                column: "DataSetVersionId",
                principalTable: "DataSetVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeSetIndicators_DataSetVersions_DataSetVersionId",
                table: "ChangeSetIndicators",
                column: "DataSetVersionId",
                principalTable: "DataSetVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeSetLocations_DataSetVersions_DataSetVersionId",
                table: "ChangeSetLocations",
                column: "DataSetVersionId",
                principalTable: "DataSetVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeSetTimePeriods_DataSetVersions_DataSetVersionId",
                table: "ChangeSetTimePeriods",
                column: "DataSetVersionId",
                principalTable: "DataSetVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataSets_DataSetVersions_LatestVersionId",
                table: "DataSets",
                column: "LatestVersionId",
                principalTable: "DataSetVersions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataSets_DataSetVersions_LatestVersionId",
                table: "DataSets");

            migrationBuilder.DropTable(
                name: "ChangeSetFilterOptions");

            migrationBuilder.DropTable(
                name: "ChangeSetFilters");

            migrationBuilder.DropTable(
                name: "ChangeSetIndicators");

            migrationBuilder.DropTable(
                name: "ChangeSetLocations");

            migrationBuilder.DropTable(
                name: "ChangeSetTimePeriods");

            migrationBuilder.DropTable(
                name: "FilterMetas");

            migrationBuilder.DropTable(
                name: "GeographicLevelMetas");

            migrationBuilder.DropTable(
                name: "IndicatorMetas");

            migrationBuilder.DropTable(
                name: "LocationMetas");

            migrationBuilder.DropTable(
                name: "TimePeriodMetas");

            migrationBuilder.DropTable(
                name: "DataSetVersions");

            migrationBuilder.DropTable(
                name: "DataSets");
        }
    }
}
