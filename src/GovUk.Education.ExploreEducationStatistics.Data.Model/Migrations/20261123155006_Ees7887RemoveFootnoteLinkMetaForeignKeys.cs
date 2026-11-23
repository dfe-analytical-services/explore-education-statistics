using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class Ees7887RemoveFootnoteLinkMetaForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_FilterFootnote_Filter_FilterId", table: "FilterFootnote");

            migrationBuilder.DropForeignKey(
                name: "FK_FilterGroupFootnote_FilterGroup_FilterGroupId",
                table: "FilterGroupFootnote"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_FilterItemFootnote_FilterItem_FilterItemId",
                table: "FilterItemFootnote"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_IndicatorFootnote_Indicator_IndicatorId",
                table: "IndicatorFootnote"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_FilterFootnote_Filter_FilterId",
                table: "FilterFootnote",
                column: "FilterId",
                principalTable: "Filter",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_FilterGroupFootnote_FilterGroup_FilterGroupId",
                table: "FilterGroupFootnote",
                column: "FilterGroupId",
                principalTable: "FilterGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_FilterItemFootnote_FilterItem_FilterItemId",
                table: "FilterItemFootnote",
                column: "FilterItemId",
                principalTable: "FilterItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_IndicatorFootnote_Indicator_IndicatorId",
                table: "IndicatorFootnote",
                column: "IndicatorId",
                principalTable: "Indicator",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
