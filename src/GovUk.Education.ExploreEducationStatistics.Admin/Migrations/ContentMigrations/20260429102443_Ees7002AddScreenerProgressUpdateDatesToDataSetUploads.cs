using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7002AddScreenerProgressUpdateDatesToDataSetUploads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ScreenerProgressLastChecked",
                table: "DataSetUploads",
                type: "datetimeoffset",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ScreenerProgressLastUpdated",
                table: "DataSetUploads",
                type: "datetimeoffset",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ScreenerProgressLastChecked", table: "DataSetUploads");

            migrationBuilder.DropColumn(name: "ScreenerProgressLastUpdated", table: "DataSetUploads");
        }
    }
}
