#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class EES6294_RemoveSoftDeletedColumnsForRolesAndInvites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPublicationRoles_Users_DeletedById",
                table: "UserPublicationRoles"
            );

            migrationBuilder.DropForeignKey(name: "FK_UserReleaseRoles_Users_DeletedById", table: "UserReleaseRoles");

            migrationBuilder.DropIndex(name: "IX_UserReleaseRoles_DeletedById", table: "UserReleaseRoles");

            migrationBuilder.DropIndex(name: "IX_UserPublicationRoles_DeletedById", table: "UserPublicationRoles");

            migrationBuilder.DropColumn(name: "Deleted", table: "UserReleaseRoles");

            migrationBuilder.DropColumn(name: "DeletedById", table: "UserReleaseRoles");

            migrationBuilder.DropColumn(name: "SoftDeleted", table: "UserReleaseRoles");

            migrationBuilder.DropColumn(name: "SoftDeleted", table: "UserReleaseInvites");

            migrationBuilder.DropColumn(name: "Deleted", table: "UserPublicationRoles");

            migrationBuilder.DropColumn(name: "DeletedById", table: "UserPublicationRoles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Deleted",
                table: "UserReleaseRoles",
                type: "datetime2",
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedById",
                table: "UserReleaseRoles",
                type: "uniqueidentifier",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "UserReleaseRoles",
                type: "bit",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "UserReleaseInvites",
                type: "bit",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "Deleted",
                table: "UserPublicationRoles",
                type: "datetime2",
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedById",
                table: "UserPublicationRoles",
                type: "uniqueidentifier",
                nullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserReleaseRoles_DeletedById",
                table: "UserReleaseRoles",
                column: "DeletedById"
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserPublicationRoles_DeletedById",
                table: "UserPublicationRoles",
                column: "DeletedById"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_UserPublicationRoles_Users_DeletedById",
                table: "UserPublicationRoles",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_UserReleaseRoles_Users_DeletedById",
                table: "UserReleaseRoles",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id"
            );
        }
    }
}
