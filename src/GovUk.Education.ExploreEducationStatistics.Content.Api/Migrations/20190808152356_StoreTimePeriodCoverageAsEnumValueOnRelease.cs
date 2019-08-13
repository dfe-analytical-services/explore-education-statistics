using System;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class StoreTimePeriodCoverageAsEnumValueOnRelease : Migration
    {
        private const string MigrationsPath = "Migrations/";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            
            migrationBuilder.AlterColumn<string>(
                name: "TimePeriodCoverage",
                table: "Releases",
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(int));
            ExecuteFile(migrationBuilder, MigrationsPath + "20190808152356_StoreTimePeriodCoverageAsEnumValueOnReleaseUp.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            ExecuteFile(migrationBuilder, MigrationsPath + "20190808152356_StoreTimePeriodCoverageAsEnumValueOnReleaseDown.sql");
            migrationBuilder.AlterColumn<int>(
                name: "TimePeriodCoverage",
                table: "Releases",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 6);
        }

        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), filename);
            migrationBuilder.Sql(File.ReadAllText(file));
        }
    }
}
