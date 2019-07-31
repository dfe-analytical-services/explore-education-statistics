using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class ChangeDependencyDirectionPublicationAndMethodology : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Methodologies_Publications_PublicationId",
                table: "Methodologies");

            migrationBuilder.DropIndex(
                name: "IX_Methodologies_PublicationId",
                table: "Methodologies");

            migrationBuilder.DropColumn(
                name: "PublicationId",
                table: "Methodologies");

            migrationBuilder.AddColumn<Guid>(
                name: "MethodologyId",
                table: "Publications",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"),
                column: "MethodologyId",
                value: new Guid("8ab41234-cc9d-4b3d-a42c-c9fce7762719"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                column: "MethodologyId",
                value: new Guid("c8c911e3-39c1-452b-801f-25bb79d1deb7"));

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                column: "MethodologyId",
                value: new Guid("caa8e56f-41d2-4129-a5c3-53b051134bd7"));

            migrationBuilder.CreateIndex(
                name: "IX_Publications_MethodologyId",
                table: "Publications",
                column: "MethodologyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Publications_Methodologies_MethodologyId",
                table: "Publications",
                column: "MethodologyId",
                principalTable: "Methodologies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Publications_Methodologies_MethodologyId",
                table: "Publications");

            migrationBuilder.DropIndex(
                name: "IX_Publications_MethodologyId",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "MethodologyId",
                table: "Publications");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicationId",
                table: "Methodologies",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("8ab41234-cc9d-4b3d-a42c-c9fce7762719"),
                column: "PublicationId",
                value: new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"));

            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("c8c911e3-39c1-452b-801f-25bb79d1deb7"),
                column: "PublicationId",
                value: new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"));

            migrationBuilder.UpdateData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("caa8e56f-41d2-4129-a5c3-53b051134bd7"),
                column: "PublicationId",
                value: new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"));

            migrationBuilder.CreateIndex(
                name: "IX_Methodologies_PublicationId",
                table: "Methodologies",
                column: "PublicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Methodologies_Publications_PublicationId",
                table: "Methodologies",
                column: "PublicationId",
                principalTable: "Publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
