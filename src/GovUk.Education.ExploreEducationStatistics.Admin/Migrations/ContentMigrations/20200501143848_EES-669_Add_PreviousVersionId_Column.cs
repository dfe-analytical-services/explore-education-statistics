using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES669_Add_PreviousVersionId_Column : Migration
    {
        private const string MigrationId = "20200501143848";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PreviousVersionId",
                table: "Releases",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.SqlFromFile("Migrations/ContentMigrations", $"{MigrationId}_Update_PreviousVersionId.sql");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_PreviousVersionId_Version",
                table: "Releases",
                columns: new[] { "PreviousVersionId", "Version" });

            migrationBuilder.AddForeignKey(
                name: "FK_Releases_Releases_PreviousVersionId",
                table: "Releases",
                column: "PreviousVersionId",
                principalTable: "Releases",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Releases_Releases_PreviousVersionId",
                table: "Releases");

            migrationBuilder.DropIndex(
                name: "IX_Releases_PreviousVersionId_Version",
                table: "Releases");

            migrationBuilder.DropColumn(
                name: "PreviousVersionId",
                table: "Releases");
        }
    }
}
