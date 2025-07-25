using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
// ReSharper disable once InconsistentNaming
public partial class EES5037_AddDataSetFileColumnsToFilesTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "DataSetFileId",
            table: "Files",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "DataSetFileVersion",
            table: "Files",
            type: "int",
            nullable: true);

        migrationBuilder.Sql("""
                             UPDATE Files
                             SET DataSetFileId = NEWID(), DataSetFileVersion = 0
                             WHERE Type = 'Data'
                             """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DataSetFileId",
            table: "Files");

        migrationBuilder.DropColumn(
            name: "DataSetFileVersion",
            table: "Files");
    }
}
