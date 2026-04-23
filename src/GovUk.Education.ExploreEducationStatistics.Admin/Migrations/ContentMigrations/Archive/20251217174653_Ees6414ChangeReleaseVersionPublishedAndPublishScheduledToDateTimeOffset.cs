using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class Ees6414ChangeReleaseVersionPublishedAndPublishScheduledToDateTimeOffset : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "Published",
            table: "ReleaseVersions",
            type: "datetimeoffset",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "PublishScheduled",
            table: "ReleaseVersions",
            type: "datetimeoffset",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "Published",
            table: "ReleaseVersions",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetimeoffset",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTime>(
            name: "PublishScheduled",
            table: "ReleaseVersions",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetimeoffset",
            oldNullable: true
        );
    }
}
