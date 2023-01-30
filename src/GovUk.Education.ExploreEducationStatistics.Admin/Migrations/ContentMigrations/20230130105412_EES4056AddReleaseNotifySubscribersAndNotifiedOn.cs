using System;
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

[ExcludeFromCodeCoverage]
public partial class EES4056AddReleaseNotifySubscribersAndNotifiedOn : Migration
{
    private const string MigrationId = "20230130105412";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "NotifiedOn",
            table: "Releases",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "NotifySubscribers",
            table: "Releases",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.SqlFromFile(MigrationConstants.ContentMigrationsPath,
            $"{MigrationId}_EES4056AddReleaseNotifySubscribersAndNotifiedOn.sql");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "NotifiedOn",
            table: "Releases");

        migrationBuilder.DropColumn(
            name: "NotifySubscribers",
            table: "Releases");
    }
}
