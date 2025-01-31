using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES3569_RenamingTotalRows : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Rename TotalRows -> ImportedRows
            migrationBuilder.RenameColumn(
                name: "TotalRows",
                table: "DataImports",
                newName: "ImportedRows");

            // 2. Rename Rows -> TotalRows
            migrationBuilder.RenameColumn(
                name: "Rows",
                table: "DataImports",
                newName: "TotalRows");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Rename TotalRows -> Rows
            migrationBuilder.RenameColumn(
                name: "TotalRows",
                table: "DataImports",
                newName: "Rows");

            // 2. Rename ImportedRows -> TotalRows
            migrationBuilder.RenameColumn(
                name: "ImportedRows",
                table: "DataImports",
                newName: "TotalRows");
        }
    }
}
