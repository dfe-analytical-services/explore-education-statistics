using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES18UpdateCommentsModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_ContentBlock_IContentBlockId",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_IContentBlockId",
                table: "Comment");

            migrationBuilder.RenameColumn("IContentBlockId",
                "Comment",
                "ContentBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_ContentBlockId",
                table: "Comment",
                column: "ContentBlockId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_ContentBlock_ContentBlockId",
                table: "Comment",
                column: "ContentBlockId",
                principalTable: "ContentBlock",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.RenameColumn("CommentText",
                "Comment",
                "Content");

            migrationBuilder.RenameColumn("Name",
                "Comment",
                "LegacyCreatedBy");

            migrationBuilder.DropColumn(
                name: "ResolvedBy",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "ResolvedOn",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Comment");

            migrationBuilder.RenameColumn("Time",
                "Comment",
                "Created");

            migrationBuilder.RenameColumn("UserId",
                "Comment",
                "CreatedById");

            migrationBuilder.AlterColumn<Guid>("CreatedById",
                "Comment",
                nullable: true);

            migrationBuilder.Sql(
                $"UPDATE dbo.Comment SET CreatedById = NULL WHERE CreatedById = '{Guid.Empty.ToString()}'");

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "Comment",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Comment",
                keyColumn: "Id",
                keyValue: new Guid("514940e6-3b84-4e1b-aa5d-d1e5fa671e1b"),
                columns: new[] {"CreatedById", "LegacyCreatedBy"},
                values: new object[] {new Guid("b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd"), null});

            migrationBuilder.CreateIndex(
                name: "IX_Comment_CreatedById",
                table: "Comment",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Users_CreatedById",
                table: "Comment",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Users_CreatedById",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_CreatedById",
                table: "Comment");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_ContentBlock_ContentBlockId",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_ContentBlockId",
                table: "Comment");

            migrationBuilder.RenameColumn("ContentBlockId",
                "Comment",
                "IContentBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_IContentBlockId",
                table: "Comment",
                column: "IContentBlockId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_ContentBlock_IContentBlockId",
                table: "Comment",
                column: "IContentBlockId",
                principalTable: "ContentBlock",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.RenameColumn("Content",
                "Comment",
                "CommentText");

            migrationBuilder.RenameColumn("LegacyCreatedBy",
                "Comment",
                "Name");

            migrationBuilder.AddColumn<string>(
                name: "ResolvedBy",
                table: "Comment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedOn",
                table: "Comment",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Comment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.RenameColumn("Created",
                "Comment",
                "Time");

            migrationBuilder.AlterColumn<Guid>("CreatedById",
                "Comment",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.RenameColumn("CreatedById",
                "Comment",
                "UserId");

            migrationBuilder.DropColumn(
                name: "Updated",
                table: "Comment");

            migrationBuilder.UpdateData(
                table: "Comment",
                keyColumn: "Id",
                keyValue: new Guid("514940e6-3b84-4e1b-aa5d-d1e5fa671e1b"),
                columns: new[] {"UserId", "Name"},
                values: new object[] {new Guid("00000000-0000-0000-0000-000000000000"), "A Test User"});
        }
    }
}