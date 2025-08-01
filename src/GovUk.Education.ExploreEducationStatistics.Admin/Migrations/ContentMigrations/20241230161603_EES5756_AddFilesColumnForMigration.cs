using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
// ReSharper disable once InconsistentNaming
public partial class EES5756_AddFilesColumnForMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "DataSetFileMetaGeogLvlMigrated",
            table: "Files",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.Sql("""
                             UPDATE [dbo].Files
                             SET DataSetFileMetaGeogLvlMigrated = 1 
                             WHERE Type != 'Data'
                             """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DataSetFileMetaGeogLvlMigrated",
            table: "Files");
    }
}
