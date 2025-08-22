#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
public partial class EES5765_RemoveFileGeogLvlMigrationColumn : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DataSetFileMetaGeogLvlMigrated",
            table: "Files");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "DataSetFileMetaGeogLvlMigrated",
            table: "Files",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }
}
