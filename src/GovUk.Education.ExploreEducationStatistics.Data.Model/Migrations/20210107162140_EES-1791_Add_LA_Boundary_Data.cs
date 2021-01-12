using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1791_Add_LA_Boundary_Data : Migration
    {
        private const string MigrationId = "20210107162140";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE dbo.geometry_columns SET f_table_catalog = 'statistics' WHERE f_table_catalog = 'master'");
            migrationBuilder.Sql("SET IDENTITY_INSERT BoundaryLevel ON");
            migrationBuilder.Sql(
                "INSERT INTO dbo.BoundaryLevel (Id, Level, Label, Published) VALUES (9, 'LA', 'Counties and Unitary Authorities April 2019 Boundaries EW BUC', '2020-07-27 00:00:00.0000000')");
            migrationBuilder.Sql("SET IDENTITY_INSERT BoundaryLevel OFF");
            migrationBuilder.SqlFromFileByLine(MigrationsPath, $"{MigrationId}_GeometryData.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DELETE FROM dbo.geometry WHERE boundary_level_id = 9");
            migrationBuilder.Sql(
                "DELETE FROM dbo.BoundaryLevel WHERE Id = 9");
            migrationBuilder.Sql(
                "UPDATE dbo.geometry_columns SET f_table_catalog = 'master' WHERE f_table_catalog = 'statistics'");
        }
    }
}