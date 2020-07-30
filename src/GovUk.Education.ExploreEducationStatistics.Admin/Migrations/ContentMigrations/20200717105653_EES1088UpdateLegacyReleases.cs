using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1088UpdateLegacyReleases : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LegacyRelease_Publications_PublicationId",
                table: "LegacyRelease");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LegacyRelease",
                table: "LegacyRelease");

            migrationBuilder.RenameTable(
                name: "LegacyRelease",
                newName: "LegacyReleases");

            migrationBuilder.RenameIndex(
                name: "IX_LegacyRelease_PublicationId",
                table: "LegacyReleases",
                newName: "IX_LegacyReleases_PublicationId");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "LegacyReleases",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "LegacyReleases",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LegacyReleases",
                table: "LegacyReleases",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LegacyReleases_Publications_PublicationId",
                table: "LegacyReleases",
                column: "PublicationId",
                principalTable: "Publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LegacyReleases_Publications_PublicationId",
                table: "LegacyReleases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LegacyReleases",
                table: "LegacyReleases");

            migrationBuilder.RenameTable(
                name: "LegacyReleases",
                newName: "LegacyRelease");

            migrationBuilder.RenameIndex(
                name: "IX_LegacyReleases_PublicationId",
                table: "LegacyRelease",
                newName: "IX_LegacyRelease_PublicationId");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "LegacyRelease",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "LegacyRelease",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddPrimaryKey(
                name: "PK_LegacyRelease",
                table: "LegacyRelease",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LegacyRelease_Publications_PublicationId",
                table: "LegacyRelease",
                column: "PublicationId",
                principalTable: "Publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
