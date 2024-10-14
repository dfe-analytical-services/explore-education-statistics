using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class EES5573_SetAnyUserIdColumnsToNullOnDeleteUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Users_CreatedById",
                table: "Comment");

            migrationBuilder.DropForeignKey(
                name: "FK_FeaturedTables_Users_CreatedById",
                table: "FeaturedTables");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_Users_CreatedById",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_GlossaryEntries_Users_CreatedById",
                table: "GlossaryEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_KeyStatistics_Users_CreatedById",
                table: "KeyStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_MethodologyNotes_Users_CreatedById",
                table: "MethodologyNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_MethodologyStatus_Users_CreatedById",
                table: "MethodologyStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseStatus_Users_CreatedById",
                table: "ReleaseStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseVersions_Users_CreatedById",
                table: "ReleaseVersions");

            migrationBuilder.DropForeignKey(
                name: "FK_Update_Users_CreatedById",
                table: "Update");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPublicationInvites_Users_CreatedById",
                table: "UserPublicationInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_UserReleaseInvites_Users_CreatedById",
                table: "UserReleaseInvites");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "UserReleaseInvites",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "UserPublicationInvites",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "ReleaseVersions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "MethodologyNotes",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "GlossaryEntries",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Users_CreatedById",
                table: "Comment",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FeaturedTables_Users_CreatedById",
                table: "FeaturedTables",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Users_CreatedById",
                table: "Files",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GlossaryEntries_Users_CreatedById",
                table: "GlossaryEntries",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_KeyStatistics_Users_CreatedById",
                table: "KeyStatistics",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MethodologyNotes_Users_CreatedById",
                table: "MethodologyNotes",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MethodologyStatus_Users_CreatedById",
                table: "MethodologyStatus",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseStatus_Users_CreatedById",
                table: "ReleaseStatus",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseVersions_Users_CreatedById",
                table: "ReleaseVersions",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Update_Users_CreatedById",
                table: "Update",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPublicationInvites_Users_CreatedById",
                table: "UserPublicationInvites",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserReleaseInvites_Users_CreatedById",
                table: "UserReleaseInvites",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Users_CreatedById",
                table: "Comment");

            migrationBuilder.DropForeignKey(
                name: "FK_FeaturedTables_Users_CreatedById",
                table: "FeaturedTables");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_Users_CreatedById",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_GlossaryEntries_Users_CreatedById",
                table: "GlossaryEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_KeyStatistics_Users_CreatedById",
                table: "KeyStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_MethodologyNotes_Users_CreatedById",
                table: "MethodologyNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_MethodologyStatus_Users_CreatedById",
                table: "MethodologyStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseStatus_Users_CreatedById",
                table: "ReleaseStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseVersions_Users_CreatedById",
                table: "ReleaseVersions");

            migrationBuilder.DropForeignKey(
                name: "FK_Update_Users_CreatedById",
                table: "Update");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPublicationInvites_Users_CreatedById",
                table: "UserPublicationInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_UserReleaseInvites_Users_CreatedById",
                table: "UserReleaseInvites");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "UserReleaseInvites",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "UserPublicationInvites",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "ReleaseVersions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "MethodologyNotes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "GlossaryEntries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Users_CreatedById",
                table: "Comment",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FeaturedTables_Users_CreatedById",
                table: "FeaturedTables",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Users_CreatedById",
                table: "Files",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GlossaryEntries_Users_CreatedById",
                table: "GlossaryEntries",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_KeyStatistics_Users_CreatedById",
                table: "KeyStatistics",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MethodologyNotes_Users_CreatedById",
                table: "MethodologyNotes",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MethodologyStatus_Users_CreatedById",
                table: "MethodologyStatus",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseStatus_Users_CreatedById",
                table: "ReleaseStatus",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseVersions_Users_CreatedById",
                table: "ReleaseVersions",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Update_Users_CreatedById",
                table: "Update",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPublicationInvites_Users_CreatedById",
                table: "UserPublicationInvites",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserReleaseInvites_Users_CreatedById",
                table: "UserReleaseInvites",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
