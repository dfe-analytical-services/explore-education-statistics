using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

[ExcludeFromCodeCoverage]
public partial class EES4058DropUnusedFieldsAfterEES4056 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "NotifiedOn",
            table: "ReleaseStatus");

        migrationBuilder.DropColumn(
            name: "NotifySubscribers",
            table: "ReleaseStatus");

        migrationBuilder.DropColumn(
            name: "DataLastPublished",
            table: "Releases");

        migrationBuilder.DropColumn(
            name: "Published",
            table: "Publications");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "NotifiedOn",
            table: "ReleaseStatus",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "NotifySubscribers",
            table: "ReleaseStatus",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<DateTime>(
            name: "DataLastPublished",
            table: "Releases",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "Published",
            table: "Publications",
            type: "datetime2",
            nullable: true);
    }
}
