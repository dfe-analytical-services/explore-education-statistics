using Microsoft.EntityFrameworkCore.Migrations;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1463_Add_Update_DropAndCreateRelease_SP : Migration
    {
        private const string MigrationId = "20201007151739";
        private const string PreviousDropAndCreateReleaseMigrationId = "20200715112018";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_DropAndCreateRelease.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousDropAndCreateReleaseMigrationId}_Routine_DropAndCreateRelease.sql");
        }
    }
}
