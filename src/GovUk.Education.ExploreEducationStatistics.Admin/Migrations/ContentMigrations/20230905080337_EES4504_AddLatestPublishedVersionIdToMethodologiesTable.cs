using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES4504_AddLatestPublishedVersionIdToMethodologiesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LatestPublishedVersionId",
                table: "Methodologies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Methodologies_LatestPublishedVersionId",
                table: "Methodologies",
                column: "LatestPublishedVersionId",
                unique: true,
                filter: "[LatestPublishedVersionId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Methodologies_MethodologyVersions_LatestPublishedVersionId",
                table: "Methodologies",
                column: "LatestPublishedVersionId",
                principalTable: "MethodologyVersions",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Methodologies_MethodologyVersions_LatestPublishedVersionId",
                table: "Methodologies");

            migrationBuilder.DropIndex(
                name: "IX_Methodologies_LatestPublishedVersionId",
                table: "Methodologies");

            migrationBuilder.DropColumn(
                name: "LatestPublishedVersionId",
                table: "Methodologies");
        }
    }
}
