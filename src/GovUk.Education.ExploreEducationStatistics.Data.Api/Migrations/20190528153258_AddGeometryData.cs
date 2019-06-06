using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class AddGeometryData : Migration
    {
        private readonly string _migrationsPath = "Migrations/";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET IDENTITY_INSERT dbo.geometry ON");
            ExecuteFile(migrationBuilder, _migrationsPath + "20190528153258_GeometryData.sql");
            migrationBuilder.Sql("SET IDENTITY_INSERT dbo.geometry OFF");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("TRUNCATE TABLE dbo.geometry");
        }

        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), filename);
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                migrationBuilder.Sql(line);
            }
        }
    }
}