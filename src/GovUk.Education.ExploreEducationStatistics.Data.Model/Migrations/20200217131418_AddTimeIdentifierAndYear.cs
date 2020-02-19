using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class AddTimeIdentifierAndYear : Migration
    {
        private const string MigrationsPath = "Migrations";
        private const string MigrationId = "20200217131418";
        private const string PreviousVersionMigrationId = "20200211122058";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Release");

            migrationBuilder.AddColumn<string>(
                name: "TimeIdentifier",
                table: "Release",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Release",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("DROP PROCEDURE dbo.DropAndCreateRelease");
            migrationBuilder.Sql("DROP TYPE dbo.ReleaseType");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Type_ReleaseType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_DropAndCreateRelease.sql");
            migrationBuilder.SqlFromFileByLine(MigrationsPath, $"{MigrationId}_Data.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.DropAndCreateRelease");
            migrationBuilder.Sql("DROP TYPE dbo.ReleaseType");

            migrationBuilder.DropColumn(
                name: "TimeIdentifier",
                table: "Release");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Release");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Release",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.SqlFromFileByLine(MigrationsPath, $"{MigrationId}_Data_Down.sql");
            // Revert to the version in the previous migration 20200103101609_TableTypes.sql
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Type_ReleaseType_Down.sql");
            // Revert to the version in the previous migration 20200211122058_Routine_DropAndCreateRelease.sql
            migrationBuilder.SqlFromFile(MigrationsPath,
                $"{PreviousVersionMigrationId}_Routine_DropAndCreateRelease.sql");
        }
    }
}