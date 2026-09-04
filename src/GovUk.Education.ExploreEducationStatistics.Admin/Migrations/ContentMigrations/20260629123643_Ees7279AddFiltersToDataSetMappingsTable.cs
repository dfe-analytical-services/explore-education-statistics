using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7279AddFiltersToDataSetMappingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilterMappings",
                table: "DataSetMappings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}"
            );

            migrationBuilder.AddColumn<string>(
                name: "UnmappedReplacementFilters",
                table: "DataSetMappings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "FilterMappings", table: "DataSetMappings");

            migrationBuilder.DropColumn(name: "UnmappedReplacementFilters", table: "DataSetMappings");
        }
    }
}
