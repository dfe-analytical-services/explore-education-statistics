#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
// ReSharper disable once InconsistentNaming
public partial class EES4932_ChangeDataImportZipFileIdToBeOneToMany : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_DataImports_ZipFileId", table: "DataImports");

        migrationBuilder.CreateIndex(name: "IX_DataImports_ZipFileId", table: "DataImports", column: "ZipFileId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_DataImports_ZipFileId", table: "DataImports");

        migrationBuilder.CreateIndex(
            name: "IX_DataImports_ZipFileId",
            table: "DataImports",
            column: "ZipFileId",
            unique: true,
            filter: "[ZipFileId] IS NOT NULL"
        );
    }
}
