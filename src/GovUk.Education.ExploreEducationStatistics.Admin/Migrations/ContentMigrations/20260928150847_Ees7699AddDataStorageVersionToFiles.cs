using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7699AddDataStorageVersionToFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataStorageVersion",
                table: "Files",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true
            );

            migrationBuilder.Sql("UPDATE dbo.Files SET DataStorageVersion = 'StatsDB' WHERE Type = 'Data'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DataStorageVersion", table: "Files");
        }
    }
}
