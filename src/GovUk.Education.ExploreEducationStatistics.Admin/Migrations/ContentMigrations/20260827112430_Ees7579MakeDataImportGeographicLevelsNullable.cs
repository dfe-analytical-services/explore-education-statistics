using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7579MakeDataImportGeographicLevelsNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GeographicLevels",
                table: "DataImports",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            // Up to now, during import, geographic levels were saved as an enum rather than a string, and since the
            // column was introduced, a couple of new geographic levels were added to the enum, making these ints
            // unreliable. Since DataImports.GeographicLevels is only ever used during import (not after) we opted
            // to set previous imports to null rather than refetch an accurate list of geog lvls for them all.
            migrationBuilder.Sql("UPDATE dbo.DataImports SET GeographicLevels = NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE dbo.DataImports SET GeographicLevels = '[]' WHERE GeographicLevels IS NULL");

            migrationBuilder.AlterColumn<string>(
                name: "GeographicLevels",
                table: "DataImports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true
            );
        }
    }
}
