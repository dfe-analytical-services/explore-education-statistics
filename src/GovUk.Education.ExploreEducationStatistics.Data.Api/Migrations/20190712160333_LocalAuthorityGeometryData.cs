using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class LocalAuthorityGeometryData : Migration
    {
        private const string _migrationsPath = "Migrations/";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET IDENTITY_INSERT dbo.geometry ON");
            ExecuteFile(migrationBuilder, _migrationsPath + "20190712160333_GeometryData.sql");
            migrationBuilder.Sql("SET IDENTITY_INSERT dbo.geometry OFF");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM dbo.geometry WHERE ogr_fid BETWEEN 10207 AND 10380");
        }

        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), filename);
            migrationBuilder.Sql(File.ReadAllText(file));
        }
    }
}
