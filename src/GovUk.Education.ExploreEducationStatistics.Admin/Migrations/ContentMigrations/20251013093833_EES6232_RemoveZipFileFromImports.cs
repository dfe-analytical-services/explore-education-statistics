using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    // ReSharper disable once InconsistentNaming
    public partial class EES6232_RemoveZipFileFromImports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_DataImports_Files_ZipFileId", table: "DataImports");

            migrationBuilder.DropIndex(name: "IX_DataImports_ZipFileId", table: "DataImports");

            migrationBuilder.DropColumn(name: "ZipFileId", table: "DataImports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ZipFileId",
                table: "DataImports",
                type: "uniqueidentifier",
                nullable: true
            );

            migrationBuilder.CreateIndex(name: "IX_DataImports_ZipFileId", table: "DataImports", column: "ZipFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataImports_Files_ZipFileId",
                table: "DataImports",
                column: "ZipFileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }
    }
}
