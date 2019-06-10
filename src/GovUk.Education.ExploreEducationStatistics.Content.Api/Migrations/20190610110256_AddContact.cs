using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class AddContact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                table: "Publications",
                nullable: true,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TeamName = table.Column<string>(nullable: true),
                    TeamEmail = table.Column<string>(nullable: true),
                    ContactName = table.Column<string>(nullable: true),
                    ContactTelNo = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Contacts",
                columns: new[] { "Id", "ContactName", "ContactTelNo", "TeamEmail", "TeamName" },
                values: new object[] { new Guid("11bb7387-e85e-4571-9669-8a760dcb004f"), "Simon Shakespeare", "0114 262 1619", "teamshakes@gmail.com", "Simon's Team" });

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                column: "ContactId",
                value: new Guid("11bb7387-e85e-4571-9669-8a760dcb004f"));

            migrationBuilder.CreateIndex(
                name: "IX_Publications_ContactId",
                table: "Publications",
                column: "ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_Publications_Contacts_ContactId",
                table: "Publications",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Publications_Contacts_ContactId",
                table: "Publications");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Publications_ContactId",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "Publications");
        }
    }
}
