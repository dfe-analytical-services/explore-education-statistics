using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class AddObservationQuery : Migration
    {
        private readonly string _migrationsPath = "GovUk.Education.ExploreEducationStatistics.Data.Api/Migrations/";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ExecuteFile(migrationBuilder, _migrationsPath + "20190507122936_TableTypes.sql");
            ExecuteFile(migrationBuilder, _migrationsPath + "20190507122936_FilteredObservations.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.FilteredObservations");
            migrationBuilder.Sql("DROP TYPE dbo.IdListIntegerType");
            migrationBuilder.Sql("DROP TYPE dbo.IdListVarcharType");
        }

        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, filename);
            migrationBuilder.Sql(File.ReadAllText(file));
        }
    }
}