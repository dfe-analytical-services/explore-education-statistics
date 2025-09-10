#nullable disable

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

[ExcludeFromCodeCoverage]
public partial class EES4666_AddReleaseParent : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "ReleaseParentId",
            table: "Releases",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "ReleaseParents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                Updated = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReleaseParents", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Releases_ReleaseParentId",
            table: "Releases",
            column: "ReleaseParentId");

        migrationBuilder.AddForeignKey(
            name: "FK_Releases_ReleaseParents_ReleaseParentId",
            table: "Releases",
            column: "ReleaseParentId",
            principalTable: "ReleaseParents",
            principalColumn: "Id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Releases_ReleaseParents_ReleaseParentId",
            table: "Releases");

        migrationBuilder.DropTable(
            name: "ReleaseParents");

        migrationBuilder.DropIndex(
            name: "IX_Releases_ReleaseParentId",
            table: "Releases");

        migrationBuilder.DropColumn(
            name: "ReleaseParentId",
            table: "Releases");
    }
}
