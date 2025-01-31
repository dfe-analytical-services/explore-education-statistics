using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES4192_RemoveBatchingFromDataImportProcess : Migration
    {
        private const string MigrationId = "20230418160635";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumBatches",
                table: "DataImports");

            migrationBuilder.DropColumn(
                name: "RowsPerBatch",
                table: "DataImports");

            migrationBuilder.RenameColumn(
                name: "ImportedRows",
                table: "DataImports",
                newName: "ExpectedImportedRows");
            
            migrationBuilder.AddColumn<int>(
                name: "ImportedRows",
                table: "DataImports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastProcessedRowIndex",
                table: "DataImports",
                type: "int",
                nullable: true);
            
            migrationBuilder.SqlFromFile(MigrationConstants.ContentMigrationsPath,
                $"{MigrationId}_EES4192_RemoveBatchingFromDataImportProcess.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImportedRows",
                table: "DataImports");

            migrationBuilder.DropColumn(
                name: "LastProcessedRowIndex",
                table: "DataImports");
            
            migrationBuilder.RenameColumn(
                name: "ExpectedImportedRows",
                table: "DataImports",
                newName: "ImportedRows");

            migrationBuilder.AddColumn<int>(
                name: "NumBatches",
                table: "DataImports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowsPerBatch",
                table: "DataImports",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
