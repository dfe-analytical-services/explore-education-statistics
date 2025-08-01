﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
public partial class EES5537_CreateNotifierUserAndGrantPermissions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
                             CREATE USER [notifier] FROM LOGIN [notifier];
                             
                             GRANT SELECT ON [dbo].[Publications] TO [notifier];
                             GRANT SELECT ON [dbo].[ExternalMethodology] TO [notifier];
                             """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP USER [notifier];");
    }
}
