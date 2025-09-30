#nullable disable

using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
// ReSharper disable once InconsistentNaming
public partial class EES5625_CopyReleaseVersionFieldsToRelease : Migration
{
    private const string MigrationId = "20241122105739";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add the new columns, initially allowing for the values to be null
        migrationBuilder.AddColumn<Guid>(
            name: "PublicationId",
            table: "Releases",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Slug",
            table: "Releases",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TimePeriodCoverage",
            table: "Releases",
            type: "nvarchar(5)",
            maxLength: 5,
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "Year",
            table: "Releases",
            type: "int",
            nullable: true);

        // Copy attributes from ReleaseVersions to Releases based on the latest versions of each release 
        migrationBuilder.SqlFromFile(
            MigrationConstants.ContentMigrationsPath,
            $"{MigrationId}_{nameof(EES5625_CopyReleaseVersionFieldsToRelease)}.sql");

        // Now that every row of Releases should have values, make the columns not nullable
        migrationBuilder.AlterColumn<string>(
            name: "PublicationId",
            table: "Releases",
            type: "uniqueidentifier",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Slug",
            table: "Releases",
            type: "nvarchar(30)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(30)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "TimePeriodCoverage",
            table: "Releases",
            type: "nvarchar(5)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(5)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Year",
            table: "Releases",
            type: "int",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        // Recreate the foreign key constraint from ReleaseVersions to Publications
        // to prevent multiple delete cascade paths
        migrationBuilder.DropForeignKey(
            name: "FK_ReleaseVersions_Publications_PublicationId",
            table: "ReleaseVersions");

        migrationBuilder.AddForeignKey(
            name: "FK_ReleaseVersions_Publications_PublicationId",
            table: "ReleaseVersions",
            column: "PublicationId",
            principalTable: "Publications",
            principalColumn: "Id",
            onDelete: ReferentialAction.NoAction);

        // Add a new foreign key constraint from Releases to Publications
        migrationBuilder.AddForeignKey(
            name: "FK_Releases_Publications_PublicationId",
            table: "Releases",
            column: "PublicationId",
            principalTable: "Publications",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        // Add a unique index on PublicationId / Year / TimePeriodCoverage
        migrationBuilder.CreateIndex(
            name: "IX_Releases_PublicationId_Year_TimePeriodCoverage",
            table: "Releases",
            columns: ["PublicationId", "Year", "TimePeriodCoverage"],
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Releases_Publications_PublicationId",
            table: "Releases");

        migrationBuilder.DropForeignKey(
            name: "FK_ReleaseVersions_Publications_PublicationId",
            table: "ReleaseVersions");

        migrationBuilder.DropIndex(
            name: "IX_Releases_PublicationId_Year_TimePeriodCoverage",
            table: "Releases");

        migrationBuilder.DropColumn(
            name: "PublicationId",
            table: "Releases");

        migrationBuilder.DropColumn(
            name: "Slug",
            table: "Releases");

        migrationBuilder.DropColumn(
            name: "TimePeriodCoverage",
            table: "Releases");

        migrationBuilder.DropColumn(
            name: "Year",
            table: "Releases");

        migrationBuilder.AddForeignKey(
            name: "FK_ReleaseVersions_Publications_PublicationId",
            table: "ReleaseVersions",
            column: "PublicationId",
            principalTable: "Publications",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
