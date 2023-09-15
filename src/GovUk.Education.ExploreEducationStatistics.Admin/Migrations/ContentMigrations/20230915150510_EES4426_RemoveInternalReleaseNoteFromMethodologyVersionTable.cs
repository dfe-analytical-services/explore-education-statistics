using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES4426_RemoveInternalReleaseNoteFromMethodologyVersionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InternalReleaseNote",
                table: "MethodologyVersions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InternalReleaseNote",
                table: "MethodologyVersions",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
