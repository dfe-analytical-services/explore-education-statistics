using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

[ExcludeFromCodeCoverage]
public partial class EES3882_AddLatestPublishedReleaseIdToPublication : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "LatestPublishedReleaseId",
            table: "Publications",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Publications_LatestPublishedReleaseId",
            table: "Publications",
            column: "LatestPublishedReleaseId",
            unique: true,
            filter: "[LatestPublishedReleaseId] IS NOT NULL");

        migrationBuilder.AddForeignKey(
            name: "FK_Publications_Releases_LatestPublishedReleaseId",
            table: "Publications",
            column: "LatestPublishedReleaseId",
            principalTable: "Releases",
            principalColumn: "Id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Publications_Releases_LatestPublishedReleaseId",
            table: "Publications");

        migrationBuilder.DropIndex(
            name: "IX_Publications_LatestPublishedReleaseId",
            table: "Publications");

        migrationBuilder.DropColumn(
            name: "LatestPublishedReleaseId",
            table: "Publications");
    }
}
