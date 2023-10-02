using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES4467_RemoveOrphanedContentBlocksAndContentSections : Migration
    {
        private const string MigrationId = "20230926093503";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(
                ContentMigrationsPath, 
                $"{MigrationId}_{nameof(EES4467_RemoveOrphanedContentBlocksAndContentSections)}.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
