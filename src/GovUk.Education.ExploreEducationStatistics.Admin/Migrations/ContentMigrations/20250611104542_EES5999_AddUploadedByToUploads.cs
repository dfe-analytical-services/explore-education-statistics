using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class EES5999_AddUploadedByToUploads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UploadedBy",
                table: "DataSetUploads",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadedBy",
                table: "DataSetUploads");
        }
    }
}
