using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7818AddRolloverSchemaFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RolloverSourceReleaseId",
                table: "Releases",
                type: "uniqueidentifier",
                nullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Releases_RolloverSourceReleaseId",
                table: "Releases",
                column: "RolloverSourceReleaseId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Releases_Releases_RolloverSourceReleaseId",
                table: "Releases",
                column: "RolloverSourceReleaseId",
                principalTable: "Releases",
                principalColumn: "Id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Releases_Releases_RolloverSourceReleaseId", table: "Releases");

            migrationBuilder.DropIndex(name: "IX_Releases_RolloverSourceReleaseId", table: "Releases");

            migrationBuilder.DropColumn(name: "RolloverSourceReleaseId", table: "Releases");
        }
    }
}
