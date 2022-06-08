using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES3321_AddDeletedAndDeletedByToUserPublicationRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "UserPublicationRoles",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "UserPublicationRoles",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "Deleted",
                table: "UserPublicationRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedById",
                table: "UserPublicationRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPublicationRoles_DeletedById",
                table: "UserPublicationRoles",
                column: "DeletedById");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPublicationRoles_Users_DeletedById",
                table: "UserPublicationRoles",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPublicationRoles_Users_DeletedById",
                table: "UserPublicationRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserPublicationRoles_DeletedById",
                table: "UserPublicationRoles");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "UserPublicationRoles");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "UserPublicationRoles");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "UserPublicationRoles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "UserPublicationRoles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
