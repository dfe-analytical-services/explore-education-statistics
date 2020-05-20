using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES175_Remove_ReleaseId_From_Subject : Migration
    {
        private const string MigrationId = "20200519093903";
        private const string PreviousVersionMigrationId = "20200323102154";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subject_Release_ReleaseId",
                table: "Subject");

            migrationBuilder.DropIndex(
                name: "IX_Subject_ReleaseId",
                table: "Subject");

            migrationBuilder.DropColumn(
                name: "ReleaseId",
                table: "Subject");
            
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_FilteredFootnotes.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReleaseId",
                table: "Subject",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Subject_ReleaseId",
                table: "Subject",
                column: "ReleaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subject_Release_ReleaseId",
                table: "Subject",
                column: "ReleaseId",
                principalTable: "Release",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousVersionMigrationId}_Routine_FilteredFootnotes.sql");
        }
    }
}
