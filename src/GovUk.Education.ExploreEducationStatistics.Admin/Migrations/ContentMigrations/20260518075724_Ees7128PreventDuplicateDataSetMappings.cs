using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7128PreventDuplicateDataSetMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DataSetMappings_OriginalDataSetId",
                table: "DataSetMappings",
                column: "OriginalDataSetId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_DataSetMappings_ReplacementDataSetId",
                table: "DataSetMappings",
                column: "ReplacementDataSetId",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_DataSetMappings_OriginalDataSetId", table: "DataSetMappings");

            migrationBuilder.DropIndex(name: "IX_DataSetMappings_ReplacementDataSetId", table: "DataSetMappings");
        }
    }
}
