using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7281ReplaceFilterMappingCandidatePools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Not a rename: the old column held only the *unclaimed* replacement filters (nested with their own
            // groups/items), whereas the new columns each hold the *complete* flat replacement catalogue for their
            // level. Existing rows are backfilled by re-querying the replacement file's filters/groups/items - see
            // the "migrate-filter-mapping-candidates" BAU endpoint - not by reinterpreting the old column's data.
            migrationBuilder.DropColumn(name: "UnmappedReplacementFilters", table: "DataSetMappings");

            migrationBuilder.AddColumn<string>(
                name: "ReplacementFilters",
                table: "DataSetMappings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]"
            );

            migrationBuilder.AddColumn<string>(
                name: "ReplacementFilterGroups",
                table: "DataSetMappings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]"
            );

            migrationBuilder.AddColumn<string>(
                name: "ReplacementFilterItems",
                table: "DataSetMappings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ReplacementFilters", table: "DataSetMappings");

            migrationBuilder.DropColumn(name: "ReplacementFilterGroups", table: "DataSetMappings");

            migrationBuilder.DropColumn(name: "ReplacementFilterItems", table: "DataSetMappings");

            migrationBuilder.AddColumn<string>(
                name: "UnmappedReplacementFilters",
                table: "DataSetMappings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]"
            );
        }
    }
}
