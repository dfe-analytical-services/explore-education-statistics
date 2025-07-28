using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

/// <inheritdoc />
public partial class EES5170_AddBoundaryDataTablePermission : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("" +
            "IF EXISTS (SELECT name " +
                "FROM [sys].[database_principals] " +
                "WHERE name = 'data') " +
            "BEGIN " +
                "GRANT SELECT ON [dbo].[BoundaryData] TO [data]; " +
            "END");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("" +
            "IF EXISTS (SELECT name " +
                "FROM [sys].[database_principals] " +
                "WHERE name = 'data') " +
            "BEGIN " +
                "REVOKE SELECT ON [dbo].[BoundaryData] TO [data]; " +
            "END");
    }
}
