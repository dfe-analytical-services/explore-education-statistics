#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
// ReSharper disable once InconsistentNaming
public partial class EES5573_CreateUserTableDeletedColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(name: "DeletedById", table: "Users", type: "uniqueidentifier", nullable: true);

        migrationBuilder.AddColumn<DateTime>(name: "SoftDeleted", table: "Users", type: "datetime2", nullable: true);

        migrationBuilder.CreateIndex(name: "IX_Users_DeletedById", table: "Users", column: "DeletedById");

        migrationBuilder.AddForeignKey(
            name: "FK_Users_Users_DeletedById",
            table: "Users",
            column: "DeletedById",
            principalTable: "Users",
            principalColumn: "Id"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_Users_Users_DeletedById", table: "Users");

        migrationBuilder.DropIndex(name: "IX_Users_DeletedById", table: "Users");

        migrationBuilder.DropColumn(name: "DeletedById", table: "Users");

        migrationBuilder.DropColumn(name: "SoftDeleted", table: "Users");
    }
}
