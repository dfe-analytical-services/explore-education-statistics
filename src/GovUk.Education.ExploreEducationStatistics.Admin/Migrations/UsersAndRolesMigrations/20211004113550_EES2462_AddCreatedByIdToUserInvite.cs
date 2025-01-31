using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.UsersAndRolesMigrations
{
    public partial class EES2462_AddCreatedByIdToUserInvite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserInvites_AspNetRoles_RoleId",
                table: "UserInvites");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserInvites");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "UserInvites",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "UserInvites",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserInvites_CreatedById",
                table: "UserInvites",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_UserInvites_AspNetUsers_CreatedById",
                table: "UserInvites",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserInvites_AspNetRoles_RoleId",
                table: "UserInvites",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserInvites_AspNetUsers_CreatedById",
                table: "UserInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_UserInvites_AspNetRoles_RoleId",
                table: "UserInvites");

            migrationBuilder.DropIndex(
                name: "IX_UserInvites_CreatedById",
                table: "UserInvites");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "UserInvites");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "UserInvites",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "UserInvites",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserInvites_AspNetRoles_RoleId",
                table: "UserInvites",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
