﻿using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1828_Routine_FilteredObservations_LengthFix : Migration
    {
        private const string MigrationId = "20210129135020";
        private const string PreviousFilteredObservationsMigrationId = "20210122131615";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_FilteredObservations.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath,
                $"{PreviousFilteredObservationsMigrationId}_Routine_FilteredObservations.sql");
        }
    }
}
