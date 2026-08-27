using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class EES7369_NullSupersedingDataSetOnDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_DataSets_DataSets_SupersedingDataSetId", table: "DataSets");

            migrationBuilder.AddForeignKey(
                name: "FK_DataSets_DataSets_SupersedingDataSetId",
                table: "DataSets",
                column: "SupersedingDataSetId",
                principalTable: "DataSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_DataSets_DataSets_SupersedingDataSetId", table: "DataSets");

            migrationBuilder.AddForeignKey(
                name: "FK_DataSets_DataSets_SupersedingDataSetId",
                table: "DataSets",
                column: "SupersedingDataSetId",
                principalTable: "DataSets",
                principalColumn: "Id"
            );
        }
    }
}
