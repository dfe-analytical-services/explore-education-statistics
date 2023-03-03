using System;
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class EES4130CreateLocationsIdsIndexedView : Migration
    {
        private const string MigrationId = "20230222152209";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationConstants.MigrationsPath,
                $"{MigrationId}_View_ObservationSubjectIdLocationId.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW dbo.vObservationSubjectIdLocationId");
        }
    }
}
