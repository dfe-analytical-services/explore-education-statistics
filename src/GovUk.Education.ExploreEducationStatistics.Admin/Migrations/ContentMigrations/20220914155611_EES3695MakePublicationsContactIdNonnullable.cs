using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES3695MakePublicationsContactIdNonnullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Publications_Contacts_ContactId",
                table: "Publications");

            migrationBuilder.AlterColumn<Guid>(
                name: "ContactId",
                table: "Publications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Publications_Contacts_ContactId",
                table: "Publications",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Publications_Contacts_ContactId",
                table: "Publications");

            migrationBuilder.AlterColumn<Guid>(
                name: "ContactId",
                table: "Publications",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Publications_Contacts_ContactId",
                table: "Publications",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id");
        }
    }
}
