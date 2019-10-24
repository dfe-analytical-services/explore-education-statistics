using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    [ExcludeFromCodeCoverage]
    public partial class MakeKeyStatisticsOptional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Releases_ContentBlock_KeyStatisticsId",
                table: "Releases");

            migrationBuilder.AlterColumn<Guid>(
                name: "KeyStatisticsId",
                table: "Releases",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_Releases_ContentBlock_KeyStatisticsId",
                table: "Releases",
                column: "KeyStatisticsId",
                principalTable: "ContentBlock",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Releases_ContentBlock_KeyStatisticsId",
                table: "Releases");

            migrationBuilder.AlterColumn<Guid>(
                name: "KeyStatisticsId",
                table: "Releases",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Releases_ContentBlock_KeyStatisticsId",
                table: "Releases",
                column: "KeyStatisticsId",
                principalTable: "ContentBlock",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
