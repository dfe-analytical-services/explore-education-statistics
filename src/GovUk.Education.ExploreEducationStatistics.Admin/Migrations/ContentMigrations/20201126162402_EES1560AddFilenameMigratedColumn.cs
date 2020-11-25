using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1560AddFilenameMigratedColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add a temporary column used to track the filename migration to guids.
            // This is can be used to visualise the progress and to filter out files if multiple runs are required.
            migrationBuilder.AddColumn<bool>(
                name: "FilenameMigrated",
                table: "ReleaseFileReferences",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilenameMigrated",
                table: "ReleaseFileReferences");
        }
    }
}
