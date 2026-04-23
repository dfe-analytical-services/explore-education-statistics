using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7044CreateDataSetMappingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataSetMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalDataSetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReplacementDataSetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IndicatorMappings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnmappedReplacementIndicators = table.Column<string>(type: "nvarchar(max)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetMappings", x => x.Id);
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DataSetMappings");
        }
    }
}
