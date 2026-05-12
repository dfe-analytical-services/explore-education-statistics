using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7021AddLocationMappingsToDataSetMappingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocationMappings",
                table: "DataSetMappings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}"
            );

            migrationBuilder.AddColumn<string>(
                name: "UnmappedReplacementLocations",
                table: "DataSetMappings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "LocationMappings", table: "DataSetMappings");

            migrationBuilder.DropColumn(name: "UnmappedReplacementLocations", table: "DataSetMappings");
        }
    }
}
