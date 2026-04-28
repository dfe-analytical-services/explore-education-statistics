using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class Ees6764MakeIndicatorMappingPlanMandatoryAndRemoveDeletedIndicatorsFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "HasDeletedIndicators", table: "DataSetVersionMappings");

            migrationBuilder.AlterColumn<string>(
                name: "IndicatorMappingPlan",
                table: "DataSetVersionMappings",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IndicatorMappingPlan",
                table: "DataSetVersionMappings",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb"
            );

            migrationBuilder.AddColumn<bool>(
                name: "HasDeletedIndicators",
                table: "DataSetVersionMappings",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );
        }
    }
}
