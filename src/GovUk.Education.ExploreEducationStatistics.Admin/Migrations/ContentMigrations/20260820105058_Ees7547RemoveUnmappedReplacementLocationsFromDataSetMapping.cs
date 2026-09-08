using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7547RemoveUnmappedReplacementLocationsFromDataSetMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "UnmappedReplacementLocations", table: "DataSetMappings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnmappedReplacementLocations",
                table: "DataSetMappings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: ""
            );
        }
    }
}
