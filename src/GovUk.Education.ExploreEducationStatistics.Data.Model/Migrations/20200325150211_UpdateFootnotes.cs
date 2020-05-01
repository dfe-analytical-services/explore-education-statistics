using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class UpdateFootnotes : Migration
    {
        private const string MigrationId = "20200325150211";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_FootnoteData.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_FootnoteData_Down.sql");
        }
    }
}
