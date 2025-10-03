using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

/// <inheritdoc />
public partial class Routine_InsertObservations : Migration
{
    private const string PreviousMigrationId = EES3067_DropObservationGeographicLevel.MigrationId;
    internal const string MigrationId = "20250926184143";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add an explicit "order by" to the data being inserted into the
        // Observation table, to make best use of its primary key that is
        // optimised for sequential ids.
        migrationBuilder.SqlFromFile(MigrationConstants.MigrationsPath,
            $"{MigrationId}_Routine_InsertObservations.sql");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.SqlFromFile(MigrationConstants.MigrationsPath,
            $"{PreviousMigrationId}_Routine_InsertObservations.sql");
    }
}
