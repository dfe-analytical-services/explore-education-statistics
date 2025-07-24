using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
// ReSharper disable once InconsistentNaming
public partial class EES5740_AddFilterHierarchiesColumnToFilesTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "FilterHierarchies",
            table: "Files",
            type: "nvarchar(max)",
            nullable: true,
            defaultValue: null);

        migrationBuilder.Sql("""
                             UPDATE [dbo].[Files]
                             SET FilterHierarchies = '[]'
                             WHERE Type = 'Data';
                             """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FilterHierarchies",
            table: "Files");
    }
}
