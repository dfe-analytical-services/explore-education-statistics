using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES4486_AlterIndexDataImportsFileId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DataImports_FileId",
                table: "DataImports");

            migrationBuilder.CreateIndex(
                name: "IX_DataImports_FileId",
                table: "DataImports",
                column: "FileId",
                unique: true)
                .Annotation("SqlServer:Include", new[] { "Status" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DataImports_FileId",
                table: "DataImports");

            migrationBuilder.CreateIndex(
                name: "IX_DataImports_FileId",
                table: "DataImports",
                column: "FileId",
                unique: true);
        }
    }
}
