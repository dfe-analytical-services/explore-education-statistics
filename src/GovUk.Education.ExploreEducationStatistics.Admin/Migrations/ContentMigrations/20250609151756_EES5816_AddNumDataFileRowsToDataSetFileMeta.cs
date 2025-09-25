#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
// ReSharper disable once InconsistentNaming
public partial class EES5816_AddNumDataFileRowsToDataSetFileMeta : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
                             UPDATE F
                             SET F.DataSetFileMeta = JSON_MODIFY(F.DataSetFileMeta, '$.NumDataFileRows', DI.TotalRows)
                             FROM Files AS F
                             INNER JOIN DataImports AS DI ON F.SubjectId = DI.SubjectId
                             WHERE F.Type = 'Data';
                             """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
                             UPDATE F
                             SET F.DataSetFileMeta = JSON_MODIFY(F.DataSetFileMeta, '$.NumDataFileRows', NULL)
                             FROM Files AS F
                             WHERE F.Type = 'Data';
                             """);
    }
}
