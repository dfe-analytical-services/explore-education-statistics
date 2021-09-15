using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES2171_AddMethodologyNotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MethodologyNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Content = table.Column<string>(nullable: false),
                    DisplayDate = table.Column<DateTime>(nullable: false),
                    MethodologyVersionId = table.Column<Guid>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<Guid>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: true),
                    UpdatedById = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodologyNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MethodologyNotes_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MethodologyNotes_MethodologyVersions_MethodologyVersionId",
                        column: x => x.MethodologyVersionId,
                        principalTable: "MethodologyVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MethodologyNotes_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MethodologyNotes_CreatedById",
                table: "MethodologyNotes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MethodologyNotes_MethodologyVersionId",
                table: "MethodologyNotes",
                column: "MethodologyVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_MethodologyNotes_UpdatedById",
                table: "MethodologyNotes",
                column: "UpdatedById");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MethodologyNotes");
        }
    }
}
