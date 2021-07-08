using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES2455_AddScheduledWithReleaseToMethodology : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ScheduledWithReleaseId",
                table: "Methodologies",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Methodologies_ScheduledWithReleaseId",
                table: "Methodologies",
                column: "ScheduledWithReleaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Methodologies_Releases_ScheduledWithReleaseId",
                table: "Methodologies",
                column: "ScheduledWithReleaseId",
                principalTable: "Releases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Methodologies_Releases_ScheduledWithReleaseId",
                table: "Methodologies");

            migrationBuilder.DropIndex(
                name: "IX_Methodologies_ScheduledWithReleaseId",
                table: "Methodologies");

            migrationBuilder.DropColumn(
                name: "ScheduledWithReleaseId",
                table: "Methodologies");
        }
    }
}
