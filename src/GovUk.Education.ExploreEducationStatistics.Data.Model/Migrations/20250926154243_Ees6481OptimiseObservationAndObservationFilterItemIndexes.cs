using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

/// <inheritdoc />
public partial class Ees6481OptimiseObservationAndObservationFilterItemIndexes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        //
        // Compress the Observation table's primary key for space-saving,
        // reduced I/O for reading and optimisation for sequential GUIDs
        // for inserts.
        //
        migrationBuilder.Sql(
            @"
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'TakesTooLongToRunOnDeploy'
                AND object_id = OBJECT_ID('dbo.Observation')
            )
            BEGIN
                ALTER INDEX PK_Observation
                ON dbo.Observation
                SET (OPTIMIZE_FOR_SEQUENTIAL_KEY = ON);

                ALTER INDEX PK_Observation
                ON dbo.Observation
                REBUILD WITH (
                    DATA_COMPRESSION = PAGE,
                    ONLINE = ON,
                    SORT_IN_TEMPDB = ON,
                    FILLFACTOR = 95
                );
            END
        "
        );

        //
        // Compress the Observation table's biggest covering index,
        // used when fetching the final set of table tool results.
        // This provides space-saving and reduced I/O for reading.
        //
        migrationBuilder.Sql(
            @"
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'TakesTooLongToRunOnDeploy'
                AND object_id = OBJECT_ID('dbo.Observation')
            )
            BEGIN
                ALTER INDEX NCI_WI_Observation_SubjectId
                ON dbo.Observation
                REBUILD WITH (
                    DATA_COMPRESSION = PAGE,
                    ONLINE = ON,
                    SORT_IN_TEMPDB = ON,
                    FILLFACTOR = 95
                );
            END
        "
        );

        //
        // Compress the IX_Observation_LocationId Observation
        // table index, used when fetching all distinct Locations used by
        // all Observations as part of the "remove orphaned Locations"
        // stored procedure.
        //
        migrationBuilder.Sql(
            @"
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'TakesTooLongToRunOnDeploy'
                AND object_id = OBJECT_ID('dbo.Observation')
            )
            BEGIN
                ALTER INDEX IX_Observation_LocationId
                ON dbo.Observation
                REBUILD WITH (
                    DATA_COMPRESSION = PAGE,
                    ONLINE = ON,
                    SORT_IN_TEMPDB = ON
                );
            END
        "
        );

        //
        // Compress the ObservationFilterItem table's primary key for
        // space-saving, reduced I/O for reading and optimisation for
        // sequential GUIDs for inserts.
        //
        migrationBuilder.Sql(
            @"
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'TakesTooLongToRunOnDeploy'
                AND object_id = OBJECT_ID('dbo.ObservationFilterItem')
            )
            BEGIN
                ALTER INDEX PK_ObservationFilterItem
                ON dbo.ObservationFilterItem
                SET (OPTIMIZE_FOR_SEQUENTIAL_KEY = ON);

                ALTER INDEX PK_ObservationFilterItem
                ON dbo.ObservationFilterItem
                REBUILD WITH (
                    DATA_COMPRESSION = PAGE,
                    ONLINE = ON,
                    SORT_IN_TEMPDB = ON,
                    FILLFACTOR = 95
                );
            END
        "
        );

        //
        // Compress the IX_ObservationFilterItem_FilterId ObservationFilterItem
        // table index, used when finding ObservationFilterItems to delete
        // Filter by Filter. This is done when deleting soft-deleted Subjects
        // and tearing down test data (through a Subject cascading delete).
        //
        migrationBuilder.Sql(
            @"
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'TakesTooLongToRunOnDeploy'
                AND object_id = OBJECT_ID('dbo.ObservationFilterItem')
            )
            BEGIN
                ALTER INDEX IX_ObservationFilterItem_FilterId
                ON dbo.ObservationFilterItem
                REBUILD WITH (
                    DATA_COMPRESSION = PAGE,
                    ONLINE = ON,
                    SORT_IN_TEMPDB = ON
                );
            END
        "
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            @"
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'TakesTooLongToRunOnDeploy'
                AND object_id = OBJECT_ID('dbo.Observation')
            )
            BEGIN
                ALTER INDEX PK_Observation
                ON dbo.Observation
                SET (OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF);

                ALTER INDEX PK_Observation
                ON dbo.Observation
                REBUILD WITH (
                    DATA_COMPRESSION = NONE,
                    ONLINE = ON,
                    SORT_IN_TEMPDB = ON
                );
            END
        "
        );

        migrationBuilder.Sql(
            @"
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'TakesTooLongToRunOnDeploy'
                AND object_id = OBJECT_ID('dbo.Observation')
            )
            BEGIN
                ALTER INDEX NCI_WI_Observation_SubjectId
                ON dbo.Observation
                REBUILD WITH (
                    DATA_COMPRESSION = NONE,
                    ONLINE = ON,
                    SORT_IN_TEMPDB = ON
                );
            END
        "
        );

        migrationBuilder.Sql(
            @"
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'TakesTooLongToRunOnDeploy'
                AND object_id = OBJECT_ID('dbo.Observation')
            )
            BEGIN
                ALTER INDEX IX_Observation_SubjectId_LocationId
                ON dbo.Observation
                REBUILD WITH (
                    DATA_COMPRESSION = NONE,
                    ONLINE = ON,
                    SORT_IN_TEMPDB = ON
                );
            END
        "
        );

        migrationBuilder.Sql(
            @"
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'TakesTooLongToRunOnDeploy'
                AND object_id = OBJECT_ID('dbo.ObservationFilterItem')
            )
            BEGIN
                ALTER INDEX PK_ObservationFilterItem
                ON dbo.ObservationFilterItem
                SET (OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF);

                ALTER INDEX PK_ObservationFilterItem
                ON dbo.ObservationFilterItem
                REBUILD WITH (
                    DATA_COMPRESSION = NONE,
                    ONLINE = ON,
                    SORT_IN_TEMPDB = ON
                );
            END
        "
        );
    }
}
