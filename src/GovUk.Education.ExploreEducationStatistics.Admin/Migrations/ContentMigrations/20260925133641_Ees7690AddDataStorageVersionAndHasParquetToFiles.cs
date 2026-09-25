using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7690AddDataStorageVersionAndHasParquetToFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataStorageVersion",
                table: "Files",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "StatsDB"
            );

            migrationBuilder.AddColumn<bool>(
                name: "HasParquet",
                table: "Files",
                type: "bit",
                nullable: false,
                defaultValue: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DataStorageVersion", table: "Files");

            migrationBuilder.DropColumn(name: "HasParquet", table: "Files");
        }
    }
}
