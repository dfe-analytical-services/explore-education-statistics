using System;
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    [ExcludeFromCodeCoverage]
    public partial class EES4486_AlterIndexDataImportsFileId: Migration
    {
        private const string PreviousRebuildIndexesMigrationId = InitialCreate_Custom.MigrationId;
        internal const string MigrationId = "20230906093324";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationConstants.MigrationsPath,
                $"{MigrationId}_Index_IXDataImportsFileId.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationConstants.MigrationsPath,
                $"Current_Index_IXDataImportsFileId.sql");
        }
    }
}
