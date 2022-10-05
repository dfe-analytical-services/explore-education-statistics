using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES3751_AddPermalink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permalinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublicationTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataSetTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReleaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permalinks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permalinks_ReleaseId",
                table: "Permalinks",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Permalinks_SubjectId",
                table: "Permalinks",
                column: "SubjectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Permalinks");
        }
    }
}
