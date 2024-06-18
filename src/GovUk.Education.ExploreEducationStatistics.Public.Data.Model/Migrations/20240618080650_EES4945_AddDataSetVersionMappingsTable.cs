using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class EES4945_AddDataSetVersionMappingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataSetVersionMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetDataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Filters = table.Column<string>(type: "jsonb", nullable: false),
                    Locations = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetVersionMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSetVersionMappings_DataSetVersions_SourceDataSetVersion~",
                        column: x => x.SourceDataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataSetVersionMappings_DataSetVersions_TargetDataSetVersion~",
                        column: x => x.TargetDataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataSetVersionMappings_SourceDataSetVersionId",
                table: "DataSetVersionMappings",
                column: "SourceDataSetVersionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataSetVersionMappings_TargetDataSetVersionId",
                table: "DataSetVersionMappings",
                column: "TargetDataSetVersionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataSetVersionMappings");
        }
    }
}
