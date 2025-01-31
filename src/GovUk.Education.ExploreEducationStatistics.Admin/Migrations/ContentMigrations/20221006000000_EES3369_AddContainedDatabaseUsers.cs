using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES3369_AddContainedDatabaseUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(ContentMigrationsPath, 
                "20221006000000_EES3369_AddContainedDatabaseUsers.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
