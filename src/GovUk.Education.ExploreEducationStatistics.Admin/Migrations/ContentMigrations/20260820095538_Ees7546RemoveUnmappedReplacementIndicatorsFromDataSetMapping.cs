using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7546RemoveUnmappedReplacementIndicatorsFromDataSetMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "UnmappedReplacementIndicators", table: "DataSetMappings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnmappedReplacementIndicators",
                table: "DataSetMappings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: ""
            );
        }
    }
}
