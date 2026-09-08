using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7578AddCsvOnlyToDataSetFileVersionGeographicLevel : Migration
    {
        private const string MigrationId = "20260828112013";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CsvOnly",
                table: "DataSetFileVersionGeographicLevels",
                type: "bit",
                nullable: true
            );

            migrationBuilder.SqlFromFile(
                MigrationConstants.ContentMigrationsPath,
                $"{MigrationId}_{nameof(Ees7578AddCsvOnlyToDataSetFileVersionGeographicLevel)}.sql"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CsvOnly", table: "DataSetFileVersionGeographicLevels");
        }
    }
}
