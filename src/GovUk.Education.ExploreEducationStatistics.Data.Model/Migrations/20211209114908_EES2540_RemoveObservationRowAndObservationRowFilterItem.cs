using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class EES2540_RemoveObservationRowAndObservationRowFilterItem : Migration
    {
        private const string MigrationId = "20211209114908";
        private const string PreviousRoutinesMigrationId = "20210512112804";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.FilteredObservationRows");
            migrationBuilder.Sql("DROP PROCEDURE dbo.GetFilteredObservations");
            migrationBuilder.Sql("DROP PROCEDURE dbo.InsertObservationRows");

            migrationBuilder.Sql("DROP TYPE dbo.ObservationRowFilterItemType");
            migrationBuilder.Sql("DROP TYPE dbo.ObservationRowType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath,
                $"{MigrationId}_TableTypes.sql");

            migrationBuilder.SqlFromFile(MigrationsPath,
                $"{PreviousRoutinesMigrationId}_Routine_FilteredObservationRows.sql");

            migrationBuilder.SqlFromFile(MigrationsPath,
                $"{PreviousRoutinesMigrationId}_Routine_GetFilteredObservations.sql");

            migrationBuilder.SqlFromFile(MigrationsPath,
                $"{PreviousRoutinesMigrationId}_Routine_InsertObservationRows.sql");
        }
    }
}
