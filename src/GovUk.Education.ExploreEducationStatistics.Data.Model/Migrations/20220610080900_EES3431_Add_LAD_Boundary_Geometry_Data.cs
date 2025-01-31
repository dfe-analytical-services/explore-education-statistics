using Microsoft.EntityFrameworkCore.Migrations;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES3431_Add_LAD_Boundary_Geometry_Data : Migration
    {
        private const string MigrationId = "20220610080900";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET IDENTITY_INSERT BoundaryLevel ON");
            migrationBuilder.Sql(
                "INSERT INTO dbo.BoundaryLevel (Id, Level, Label, Published, Created) " + 
                "VALUES (11, 'LAD', 'Local Authority Districts (December 2021) UK BUC', '2021-12-15 11:39:00.0000000', '2022-06-10 00:00:00.0000000')");
            migrationBuilder.Sql("SET IDENTITY_INSERT BoundaryLevel OFF");
            migrationBuilder.SqlFromFileByLine(MigrationsPath, $"{MigrationId}_GeometryData.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM dbo.geometry WHERE boundary_level_id = 11");
            migrationBuilder.Sql("DELETE FROM dbo.BoundaryLevel WHERE Id = 11");
        }
    }
}
