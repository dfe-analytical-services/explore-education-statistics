using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    [ExcludeFromCodeCoverage]
    // ReSharper disable once InconsistentNaming
    /// <inheritdoc />
    public partial class EES6232_RemoveFileSourceColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Files_Files_SourceId", table: "Files");

            migrationBuilder.DropIndex(name: "IX_Files_SourceId", table: "Files");

            migrationBuilder.DropColumn(name: "SourceId", table: "Files");

            migrationBuilder.Sql("DELETE FROM [Files] WHERE [Type] = 'BulkDataZip' OR [Type] = 'DataZip'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceId",
                table: "Files",
                type: "uniqueidentifier",
                nullable: true
            );

            migrationBuilder.CreateIndex(name: "IX_Files_SourceId", table: "Files", column: "SourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Files_SourceId",
                table: "Files",
                column: "SourceId",
                principalTable: "Files",
                principalColumn: "Id"
            );
        }
    }
}
