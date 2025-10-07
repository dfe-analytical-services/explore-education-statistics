using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

/// <inheritdoc />
public partial class Ees6481AddFilterItemIdObservationIdIndexToObservationFilterItemTable
    : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            @"
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'TakesTooLongToRunOnDeploy')
            )
            BEGIN
                CREATE INDEX IX_ObservationFilterItem_FilterItemId_ObservationId
                ON dbo.ObservationFilterItem (FilterItemId, ObservationId)
                WITH (
                    DATA_COMPRESSION = PAGE,
                    SORT_IN_TEMPDB = ON,
                    ONLINE = ON
                )
            END
        "
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) { }
}
