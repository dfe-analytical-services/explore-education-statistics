using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES2758_Add_LA_Boundary_Data : Migration
    {
        private const string MigrationId = "20211012154859";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET IDENTITY_INSERT BoundaryLevel ON");
            migrationBuilder.Sql(
                "INSERT INTO dbo.BoundaryLevel (Id, Level, Label, Published) VALUES (10, 'LA', 'Counties and Unitary Authorities (May 2021) UK BUC', '2021-09-15 19:01:00.0000000')");
            migrationBuilder.Sql("SET IDENTITY_INSERT BoundaryLevel OFF");
            migrationBuilder.SqlFromFileByLine(MigrationsPath, $"{MigrationId}_GeometryData.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DELETE FROM dbo.geometry WHERE boundary_level_id = 10");
            migrationBuilder.Sql(
                "DELETE FROM dbo.BoundaryLevel WHERE Id = 10");
        }
    }
}
