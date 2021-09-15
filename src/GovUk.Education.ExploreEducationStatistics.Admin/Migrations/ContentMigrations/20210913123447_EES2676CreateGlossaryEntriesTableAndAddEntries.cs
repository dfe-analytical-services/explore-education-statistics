using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES2676CreateGlossaryEntriesTableAndAddEntries : Migration
    {
        private const string GlossaryEntriesSqlFile = "20210913123447_InitialGlossaryEntries.sql";
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GlossaryEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    Slug = table.Column<string>(nullable: false),
                    Body = table.Column<string>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlossaryEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlossaryEntries_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GlossaryEntries_CreatedById",
                table: "GlossaryEntries",
                column: "CreatedById");

            migrationBuilder.SqlFromFile(ContentMigrationsPath, GlossaryEntriesSqlFile);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlossaryEntries");
        }
    }
}
