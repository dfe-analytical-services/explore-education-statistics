using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1294SetPublicationPublishedDates : Migration
    {
        private const string MigrationId = "20200814010041";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(ContentMigrationsPath,
                $"{MigrationId}_{nameof(EES1294SetPublicationPublishedDates)}.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE dbo.Publications SET Published = NULL WHERE 1 = 1;");
        }
    }
}