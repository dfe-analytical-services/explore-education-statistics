using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class EES2401_RemoveUnusedTableTypes : Migration
    {
        private const string MigrationId = "20211209111322";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TYPE dbo.PublicationType");
            migrationBuilder.Sql("DROP TYPE dbo.ThemeType");
            migrationBuilder.Sql("DROP TYPE dbo.TopicType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_TableTypes.sql");
        }
    }
}
