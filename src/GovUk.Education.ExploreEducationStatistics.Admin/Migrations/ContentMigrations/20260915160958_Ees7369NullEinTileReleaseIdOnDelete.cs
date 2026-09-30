using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7369NullEinTileReleaseIdOnDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_EinTiles_Releases_ReleaseId", table: "EinTiles");

            migrationBuilder.AddForeignKey(
                name: "FK_EinTiles_Releases_ReleaseId",
                table: "EinTiles",
                column: "ReleaseId",
                principalTable: "Releases",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_EinTiles_Releases_ReleaseId", table: "EinTiles");

            migrationBuilder.AddForeignKey(
                name: "FK_EinTiles_Releases_ReleaseId",
                table: "EinTiles",
                column: "ReleaseId",
                principalTable: "Releases",
                principalColumn: "Id"
            );
        }
    }
}
