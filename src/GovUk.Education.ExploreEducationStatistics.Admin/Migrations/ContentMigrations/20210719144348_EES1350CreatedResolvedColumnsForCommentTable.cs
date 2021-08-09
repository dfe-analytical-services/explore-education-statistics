using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1350CreatedResolvedColumnsForCommentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Resolved",
                table: "Comment",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResolvedById",
                table: "Comment",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comment_ResolvedById",
                table: "Comment",
                column: "ResolvedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Users_ResolvedById",
                table: "Comment",
                column: "ResolvedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Users_ResolvedById",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_ResolvedById",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "Resolved",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "ResolvedById",
                table: "Comment");
        }
    }
}
