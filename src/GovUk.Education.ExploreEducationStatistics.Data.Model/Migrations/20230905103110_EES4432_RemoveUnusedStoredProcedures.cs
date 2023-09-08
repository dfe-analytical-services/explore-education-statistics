using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

[ExcludeFromCodeCoverage]
public partial class EES4432_RemoveUnusedStoredProcedures : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Remove stored procedures missed by 20211209114908_EES2540_RemoveObservationRowAndObservationRowFilterItem
        migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.MigrateObservationsAndObservationFilterItems");
        migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.RemoveSoftDeletedSubjectsAndObservationRows");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
