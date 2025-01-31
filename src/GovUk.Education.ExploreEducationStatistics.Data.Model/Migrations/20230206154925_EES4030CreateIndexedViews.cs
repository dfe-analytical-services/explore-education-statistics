using System;
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES4030CreateIndexedViews : Migration
    {
        private const string MigrationId = "20230206154925";
    
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationConstants.MigrationsPath,
                $"{MigrationId}_View_ObservationSubjectIdGeographicLevel.sql");

            migrationBuilder.SqlFromFile(MigrationConstants.MigrationsPath,
                $"{MigrationId}_View_ObservationSubjectIdYearTimeIdentifier.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW dbo.vObservationSubjectIdGeographicLevel");
            migrationBuilder.Sql("DROP VIEW dbo.vObservationSubjectIdYearTimeIdentifier");
        }
    }
}
