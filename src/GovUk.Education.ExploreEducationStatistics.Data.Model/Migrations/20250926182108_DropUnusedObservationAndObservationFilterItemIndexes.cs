using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

/// <inheritdoc />
public partial class DropUnusedObservationAndObservationFilterItemIndexes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // This index appeared unused by any queries on Prod.
        migrationBuilder.Sql(@"
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'IX_Observation_Year'
                  AND object_id = OBJECT_ID('dbo.Observation')
            )
            BEGIN
                DROP INDEX IX_Observation_Year
                ON dbo.Observation
            END
        ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
