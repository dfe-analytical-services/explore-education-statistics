using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1936RemoveFileBlobPathMigratedFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivateBlobPathMigrated",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "PublicBlobPathMigrated",
                table: "Files");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PrivateBlobPathMigrated",
                table: "Files",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PublicBlobPathMigrated",
                table: "Files",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
