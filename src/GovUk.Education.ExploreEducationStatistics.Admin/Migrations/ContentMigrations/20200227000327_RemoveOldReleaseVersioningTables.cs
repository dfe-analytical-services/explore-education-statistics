using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class RemoveOldReleaseVersioningTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReleaseSummaryVersions");

            migrationBuilder.DropTable(
                name: "ReleaseSummaries");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReleaseSummaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReleaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleaseSummaries_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseSummaryVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextReleaseDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublishScheduled = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReleaseName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReleaseSummaryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimePeriodCoverage = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    TypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseSummaryVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleaseSummaryVersions_ReleaseSummaries_ReleaseSummaryId",
                        column: x => x.ReleaseSummaryId,
                        principalTable: "ReleaseSummaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseSummaryVersions_ReleaseTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "ReleaseTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ReleaseSummaries",
                columns: new[] { "Id", "ReleaseId" },
                values: new object[] { new Guid("1bf7c51f-4d12-4697-8868-455760a887a7"), new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") });

            migrationBuilder.InsertData(
                table: "ReleaseSummaries",
                columns: new[] { "Id", "ReleaseId" },
                values: new object[] { new Guid("06c45b1e-533d-4c95-900b-62beb4620f59"), new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") });

            migrationBuilder.InsertData(
                table: "ReleaseSummaries",
                columns: new[] { "Id", "ReleaseId" },
                values: new object[] { new Guid("c6e08ed3-d93a-410a-9e7e-600f2cf25725"), new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717") });

            migrationBuilder.InsertData(
                table: "ReleaseSummaryVersions",
                columns: new[] { "Id", "Created", "NextReleaseDate", "PublishScheduled", "ReleaseName", "ReleaseSummaryId", "Slug", "TimePeriodCoverage", "TypeId" },
                values: new object[] { new Guid("420ca58e-278b-456b-9031-fe74a6966159"), new DateTime(2018, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "2016", new Guid("1bf7c51f-4d12-4697-8868-455760a887a7"), "2016-17", "AY", new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c") });

            migrationBuilder.InsertData(
                table: "ReleaseSummaryVersions",
                columns: new[] { "Id", "Created", "NextReleaseDate", "PublishScheduled", "ReleaseName", "ReleaseSummaryId", "Slug", "TimePeriodCoverage", "TypeId" },
                values: new object[] { new Guid("04adfe47-9057-4abd-a0e8-5a6ac56e1560"), new DateTime(2018, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "2016", new Guid("06c45b1e-533d-4c95-900b-62beb4620f59"), "2016-17", "AY", new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c") });

            migrationBuilder.InsertData(
                table: "ReleaseSummaryVersions",
                columns: new[] { "Id", "Created", "NextReleaseDate", "PublishScheduled", "ReleaseName", "ReleaseSummaryId", "Slug", "TimePeriodCoverage", "TypeId" },
                values: new object[] { new Guid("c6e08ed3-d93a-410a-9e7e-600f2cf25725"), new DateTime(2018, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "2018", new Guid("c6e08ed3-d93a-410a-9e7e-600f2cf25725"), "2018", "AY", new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c") });

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseSummaries_ReleaseId",
                table: "ReleaseSummaries",
                column: "ReleaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseSummaryVersions_ReleaseSummaryId",
                table: "ReleaseSummaryVersions",
                column: "ReleaseSummaryId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseSummaryVersions_TypeId",
                table: "ReleaseSummaryVersions",
                column: "TypeId");
        }
    }
}
