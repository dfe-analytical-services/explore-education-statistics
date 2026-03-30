using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class Ees6764AddIndicatorMappingPlanColumnToDataSetVersionMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IndicatorMappingPlan",
                table: "DataSetVersionMappings",
                type: "jsonb",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "IndicatorMappingsComplete",
                table: "DataSetVersionMappings",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IndicatorMappingPlan", table: "DataSetVersionMappings");

            migrationBuilder.DropColumn(name: "IndicatorMappingsComplete", table: "DataSetVersionMappings");
        }
    }
}
