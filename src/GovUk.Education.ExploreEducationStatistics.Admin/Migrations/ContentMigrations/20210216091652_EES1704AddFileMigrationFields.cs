using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1704AddFileMigrationFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add temporary columns used to track the migration of blobs to the new structure.
            // Continue to return blobs for files that haven't been migrated yet by switching the result of the File.Path() functions based on these values.
            // These fields can also be used to visualise the progress and to filter out migrated files if multiple runs are required.

            migrationBuilder.AddColumn<bool>(
                name: "PrivateBlobPathMigrated",
                table: "Files",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PublicBlobPathMigrated",
                table: "Files",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivateBlobPathMigrated",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "PublicBlobPathMigrated",
                table: "Files");
        }
    }
}
