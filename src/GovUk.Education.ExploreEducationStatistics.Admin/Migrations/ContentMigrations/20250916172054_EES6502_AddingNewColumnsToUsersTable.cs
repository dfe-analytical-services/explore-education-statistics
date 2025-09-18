using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
public partial class EES6502_AddingNewColumnsToUsersTable : Migration
{
    private const string MigrationId = "20250916172054";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "LastName",
            table: "Users",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "FirstName",
            table: "Users",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Email",
            table: "Users",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        // Add 'Active' as non-nullable with a default of 'false'.
        // For existing users, we will set this to 'true' in the data migration step below IF they are not soft-deleted.
        migrationBuilder.AddColumn<bool>(
            name: "Active",
            table: "Users",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "Created",
            table: "Users",
            type: "datetimeoffset",
            nullable: false,
            defaultValue: DateTimeOffset.MinValue);

        // Add 'CreatedById' as nullable first, so that we don't break foreign key constraints while we populate existing rows
        migrationBuilder.AddColumn<Guid>(
            name: "CreatedById",
            table: "Users",
            type: "uniqueidentifier",
            nullable: true);

        // Add 'RoleId' as nullable first, so that we don't break foreign key constraints while we populate existing rows
        migrationBuilder.AddColumn<string>(
            name: "RoleId",
            table: "Users",
            type: "nvarchar(450)",
            nullable: true);

        // Migrate 'RoleId', 'Created' and 'CreatedById' values for existing Users
        migrationBuilder.SqlFromFile(
            MigrationConstants.ContentMigrationsPath,
            $"{MigrationId}_{nameof(EES6502_AddingNewColumnsToUsersTable)}.sql");

        // Alter 'RoleId' to non-nullable now that existing rows are populated
        migrationBuilder.AlterColumn<string>(
            name: "RoleId",
            table: "Users",
            type: "nvarchar(450)",
            nullable: false);

        // Alter 'CreatedById' to non-nullable now that existing rows are populated
        migrationBuilder.AlterColumn<string>(
            name: "CreatedById",
            table: "Users",
            type: "uniqueidentifier",
            nullable: false);

        migrationBuilder.CreateIndex(
            name: "IX_Users_CreatedById",
            table: "Users",
            column: "CreatedById");

        migrationBuilder.CreateIndex(
            name: "IX_Users_RoleId",
            table: "Users",
            column: "RoleId");

        migrationBuilder.AddForeignKey(
            name: "FK_Users_AspNetRoles_RoleId",
            table: "Users",
            column: "RoleId",
            principalTable: "AspNetRoles",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Users_Users_CreatedById",
            table: "Users",
            column: "CreatedById",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Users_AspNetRoles_RoleId",
            table: "Users");

        migrationBuilder.DropForeignKey(
            name: "FK_Users_Users_CreatedById",
            table: "Users");

        migrationBuilder.DropIndex(
            name: "IX_Users_CreatedById",
            table: "Users");

        migrationBuilder.DropIndex(
            name: "IX_Users_RoleId",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "Active",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "Created",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "CreatedById",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "RoleId",
            table: "Users");

        migrationBuilder.AlterColumn<string>(
            name: "LastName",
            table: "Users",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "FirstName",
            table: "Users",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "Email",
            table: "Users",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");
    }
}
