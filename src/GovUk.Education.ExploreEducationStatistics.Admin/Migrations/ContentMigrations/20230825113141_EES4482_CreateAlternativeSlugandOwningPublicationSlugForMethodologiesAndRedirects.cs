using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES4482_CreateAlternativeSlugandOwningPublicationSlugForMethodologiesAndRedirects : Migration
    {
        private const string MigrationId = "20230825113141";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "Methodologies",
                newName: "OwningPublicationSlug");

            migrationBuilder.AddColumn<string>(
                name: "AlternativeSlug",
                table: "MethodologyVersions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MethodologyRedirects",
                columns: table => new
                {
                    Slug = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MethodologyVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodologyRedirects", x => new { x.MethodologyVersionId, x.Slug });
                    table.ForeignKey(
                        name: "FK_MethodologyRedirects_MethodologyVersions_MethodologyVersionId",
                        column: x => x.MethodologyVersionId,
                        principalTable: "MethodologyVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.SqlFromFile(
                MigrationConstants.ContentMigrationsPath,
                $"{MigrationId}_MigrateMethodologyOwningPublicationSlug.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MethodologyRedirects");

            migrationBuilder.DropColumn(
                name: "AlternativeSlug",
                table: "MethodologyVersions");

            migrationBuilder.RenameColumn(
                name: "OwningPublicationSlug",
                table: "Methodologies",
                newName: "Slug");
        }
    }
}
