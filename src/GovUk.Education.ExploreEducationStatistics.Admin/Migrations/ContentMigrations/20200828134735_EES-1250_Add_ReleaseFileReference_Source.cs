using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1250_Add_ReleaseFileReference_Source : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceId",
                table: "ReleaseFileReferences",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseFileReferences_SourceId",
                table: "ReleaseFileReferences",
                column: "SourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseFileReferences_ReleaseFileReferences_SourceId",
                table: "ReleaseFileReferences",
                column: "SourceId",
                principalTable: "ReleaseFileReferences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseFileReferences_ReleaseFileReferences_SourceId",
                table: "ReleaseFileReferences");

            migrationBuilder.DropIndex(
                name: "IX_ReleaseFileReferences_SourceId",
                table: "ReleaseFileReferences");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "ReleaseFileReferences");
        }
    }
}
