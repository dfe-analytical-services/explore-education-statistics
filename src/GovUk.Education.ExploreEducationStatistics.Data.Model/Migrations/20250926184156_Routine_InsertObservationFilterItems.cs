using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

/// <inheritdoc />
public partial class Routine_InsertObservationFilterItems : Migration
{
    private const string PreviousMigrationId = InitialCreate_Custom.MigrationId;
    internal const string MigrationId = "20250926184156";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add an explicit "order by" to the data being inserted into the
        // ObservationFilterItem table, to make best use of its primary
        // key that is optimised for sequential ids.
        migrationBuilder.SqlFromFile(MigrationConstants.MigrationsPath,
            $"{MigrationId}_Routine_InsertObservationFilterItems.sql");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.SqlFromFile(MigrationConstants.MigrationsPath,
            $"{PreviousMigrationId}_Routine_InsertObservationFilterItems.sql");
    }
}
