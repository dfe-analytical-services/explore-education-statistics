using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.UsersAndRolesMigrations;

/// <inheritdoc />
public partial class EES2468_AddSetNullToUserInviteCreatedById : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_UserInvites_AspNetUsers_CreatedById",
            table: "UserInvites");

        migrationBuilder.AddForeignKey(
            name: "FK_UserInvites_AspNetUsers_CreatedById",
            table: "UserInvites",
            column: "CreatedById",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_UserInvites_AspNetUsers_CreatedById",
            table: "UserInvites");

        migrationBuilder.AddForeignKey(
            name: "FK_UserInvites_AspNetUsers_CreatedById",
            table: "UserInvites",
            column: "CreatedById",
            principalTable: "AspNetUsers",
            principalColumn: "Id");
    }
}
