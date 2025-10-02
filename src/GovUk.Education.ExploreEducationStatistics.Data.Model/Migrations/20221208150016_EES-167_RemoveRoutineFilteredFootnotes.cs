using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

[ExcludeFromCodeCoverage]
public partial class EES167_RemoveRoutineFilteredFootnotes : Migration
{
    private const string MigrationId = "20221208150016";
    private const string PreviousRoutineFilteredFootnotesMigrationId = InitialCreate_Custom.MigrationId;

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP PROCEDURE dbo.FilteredFootnotes");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.SqlFromFile(
            MigrationConstants.MigrationsPath,
            $"{PreviousRoutineFilteredFootnotesMigrationId}_Routine_FilteredFootnotes.sql"
        );
    }
}
