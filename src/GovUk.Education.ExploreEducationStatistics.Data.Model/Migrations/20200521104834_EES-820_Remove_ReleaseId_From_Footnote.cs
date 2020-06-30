using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES820_Remove_ReleaseId_From_Footnote : Migration
    {
        private const string MigrationId = "20200521104834";
        private const string PreviousVersionMigrationId = "20200519093903";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Footnote_Release_ReleaseId",
                table: "Footnote");

            migrationBuilder.DropIndex(
                name: "IX_Footnote_ReleaseId",
                table: "Footnote");

            migrationBuilder.DropColumn(
                name: "ReleaseId",
                table: "Footnote");
            
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_FilteredFootnotes.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReleaseId",
                table: "Footnote",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Footnote_ReleaseId",
                table: "Footnote",
                column: "ReleaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Footnote_Release_ReleaseId",
                table: "Footnote",
                column: "ReleaseId",
                principalTable: "Release",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousVersionMigrationId}_Routine_FilteredFootnotes.sql");
        }
    }
}
