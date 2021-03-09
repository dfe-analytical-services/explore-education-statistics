using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1836_AddObservationRowConstraintsForImportPerformance : Migration
    {
        private const string MigrationId = "20210302020000";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, 
                $"{MigrationId}_AddObservationRowConstraintsForImportPerformance.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
