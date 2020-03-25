using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class AddVersioningColumnsToRelease : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Releases",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Releases",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalId",
                table: "Releases",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Releases",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.Sql(
                @"
                UPDATE Releases SET OriginalId = Id, Version = 0, Created = GETDATE(), CreatedById = 
                    (
                    SELECT MIN(u.Id) 
                    FROM AspNetUsers u
                    JOIN AspNetUserRoles r 
                        ON r.UserId = u.Id 
                        AND r.RoleId = 'cf67b697-bddd-41bd-86e0-11b7e11d99b3'
                    )
                ");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_CreatedById",
                table: "Releases",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_OriginalId_Version",
                table: "Releases",
                columns: new[] { "OriginalId", "Version" });

            migrationBuilder.AddForeignKey(
                name: "FK_Releases_Users_CreatedById",
                table: "Releases",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Releases_Releases_OriginalId",
                table: "Releases",
                column: "OriginalId",
                principalTable: "Releases",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Releases_Users_CreatedById",
                table: "Releases");

            migrationBuilder.DropForeignKey(
                name: "FK_Releases_Releases_OriginalId",
                table: "Releases");

            migrationBuilder.DropIndex(
                name: "IX_Releases_CreatedById",
                table: "Releases");

            migrationBuilder.DropIndex(
                name: "IX_Releases_OriginalId_Version",
                table: "Releases");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Releases");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Releases");

            migrationBuilder.DropColumn(
                name: "OriginalId",
                table: "Releases");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Releases");
        }
    }
}
