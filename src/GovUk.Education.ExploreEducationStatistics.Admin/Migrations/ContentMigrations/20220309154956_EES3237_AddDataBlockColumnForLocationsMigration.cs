using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES3237_AddDataBlockColumnForLocationsMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DataBlock_TableHeaderCountChanged",
                table: "ContentBlock",
                type: "bit",
                nullable: true,
                defaultValue: false);

            // We will need to re-run the migration of data blocks to calculate a value for 'TableHeaderCountChanged'.
            // Reset the migration status of all data blocks that are already migrated.
            // Migrating them again will be safe since the chart/query/table pre-migration remain the same post-migration.
            // The new values are written to temporary columns which are safe to overwrite until EES-2955.
            migrationBuilder.Sql(
                "UPDATE dbo.ContentBlock SET DataBlock_LocationsMigrated = 0 WHERE Type = 'DataBlock' AND DataBlock_LocationsMigrated = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataBlock_TableHeaderCountChanged",
                table: "ContentBlock");
        }
    }
}
