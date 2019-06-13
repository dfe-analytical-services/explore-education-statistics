using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class AlterReleaseIdType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subject_Release_ReleaseId",
                table: "Subject");

            migrationBuilder.DropIndex(
                name: "IX_Subject_ReleaseId",
                table: "Subject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Release",
                table: "Release");

            migrationBuilder.DropColumn(
                name: "ReleaseId",
                table: "Subject");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Release");

            migrationBuilder.AddColumn<Guid>(
                name: "ReleaseIdent",
                table: "Subject",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Ident",
                table: "Release",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Release",
                table: "Release",
                column: "Ident");

            migrationBuilder.CreateIndex(
                name: "IX_Subject_ReleaseIdent",
                table: "Subject",
                column: "ReleaseIdent");

            migrationBuilder.AddForeignKey(
                name: "FK_Subject_Release_ReleaseIdent",
                table: "Subject",
                column: "ReleaseIdent",
                principalTable: "Release",
                principalColumn: "Ident",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subject_Release_ReleaseIdent",
                table: "Subject");

            migrationBuilder.DropIndex(
                name: "IX_Subject_ReleaseIdent",
                table: "Subject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Release",
                table: "Release");

            migrationBuilder.DropColumn(
                name: "ReleaseIdent",
                table: "Subject");

            migrationBuilder.DropColumn(
                name: "Ident",
                table: "Release");

            migrationBuilder.AddColumn<long>(
                name: "ReleaseId",
                table: "Subject",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "Release",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Release",
                table: "Release",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Subject_ReleaseId",
                table: "Subject",
                column: "ReleaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subject_Release_ReleaseId",
                table: "Subject",
                column: "ReleaseId",
                principalTable: "Release",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
