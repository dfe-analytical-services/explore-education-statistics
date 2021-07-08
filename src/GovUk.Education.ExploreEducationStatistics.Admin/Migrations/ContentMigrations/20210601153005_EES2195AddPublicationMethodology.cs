using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES2195AddPublicationMethodology : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Methodologies SET Content = '[]' WHERE Content IS NULL");
            migrationBuilder.Sql("UPDATE Methodologies SET Annexes = '[]' WHERE Annexes IS NULL");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Methodologies",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Methodologies",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Annexes",
                table: "Methodologies",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Methodologies",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Methodologies",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MethodologyParentId",
                table: "Methodologies",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreviousVersionId",
                table: "Methodologies",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Methodologies",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MethodologyParents",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodologyParents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublicationMethodologies",
                columns: table => new
                {
                    PublicationId = table.Column<Guid>(nullable: false),
                    MethodologyParentId = table.Column<Guid>(nullable: false),
                    Owner = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationMethodologies", x => new { x.PublicationId, x.MethodologyParentId });
                    table.ForeignKey(
                        name: "FK_PublicationMethodologies_MethodologyParents_MethodologyParentId",
                        column: x => x.MethodologyParentId,
                        principalTable: "MethodologyParents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PublicationMethodologies_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Methodologies_CreatedById",
                table: "Methodologies",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Methodologies_MethodologyParentId",
                table: "Methodologies",
                column: "MethodologyParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Methodologies_PreviousVersionId",
                table: "Methodologies",
                column: "PreviousVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicationMethodologies_MethodologyParentId",
                table: "PublicationMethodologies",
                column: "MethodologyParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Methodologies_Users_CreatedById",
                table: "Methodologies",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Methodologies_MethodologyParents_MethodologyParentId",
                table: "Methodologies",
                column: "MethodologyParentId",
                principalTable: "MethodologyParents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Methodologies_Methodologies_PreviousVersionId",
                table: "Methodologies",
                column: "PreviousVersionId",
                principalTable: "Methodologies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Methodologies_Users_CreatedById",
                table: "Methodologies");

            migrationBuilder.DropForeignKey(
                name: "FK_Methodologies_MethodologyParents_MethodologyParentId",
                table: "Methodologies");

            migrationBuilder.DropForeignKey(
                name: "FK_Methodologies_Methodologies_PreviousVersionId",
                table: "Methodologies");

            migrationBuilder.DropTable(
                name: "PublicationMethodologies");

            migrationBuilder.DropTable(
                name: "MethodologyParents");

            migrationBuilder.DropIndex(
                name: "IX_Methodologies_CreatedById",
                table: "Methodologies");

            migrationBuilder.DropIndex(
                name: "IX_Methodologies_MethodologyParentId",
                table: "Methodologies");

            migrationBuilder.DropIndex(
                name: "IX_Methodologies_PreviousVersionId",
                table: "Methodologies");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Methodologies");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Methodologies");

            migrationBuilder.DropColumn(
                name: "MethodologyParentId",
                table: "Methodologies");

            migrationBuilder.DropColumn(
                name: "PreviousVersionId",
                table: "Methodologies");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Methodologies");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Methodologies",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Methodologies",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Annexes",
                table: "Methodologies",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
