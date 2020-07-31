using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1182AddContentBlockBody : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "ContentBlock",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql("UPDATE dbo.ContentBlock SET Body = HtmlBlock_Body WHERE Type = 'HtmlBlock'");
            migrationBuilder.Sql("UPDATE dbo.ContentBlock SET Body = MarkDownBlock_Body WHERE Type = 'MarkDownBlock'");

            migrationBuilder.DropColumn(
                name: "HtmlBlock_Body",
                table: "ContentBlock");
            
            migrationBuilder.DropColumn(
                name: "MarkDownBlock_Body",
                table: "ContentBlock");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HtmlBlock_Body",
                table: "ContentBlock",
                type: "nvarchar(max)",
                nullable: true);
            
            migrationBuilder.AddColumn<string>(
                name: "MarkDownBlock_Body",
                table: "ContentBlock",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql("UPDATE dbo.ContentBlock SET HtmlBlock_Body = Body WHERE Type = 'HtmlBlock'");
            migrationBuilder.Sql("UPDATE dbo.ContentBlock SET MarkDownBlock_Body = Body WHERE Type = 'MarkDownBlock'");
            
            migrationBuilder.DropColumn(
                name: "Body",
                table: "ContentBlock");
        }
    }
}
