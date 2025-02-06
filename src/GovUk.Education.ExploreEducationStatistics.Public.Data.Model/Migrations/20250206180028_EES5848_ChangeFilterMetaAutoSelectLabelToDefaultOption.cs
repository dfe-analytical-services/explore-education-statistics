using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class EES5848_ChangeFilterMetaAutoSelectLabelToDefaultOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<int>(
                name: "DefaultOptionId",
                table: "FilterMetas",
                type: "integer",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FilterMetas_DefaultOptionId",
                table: "FilterMetas",
                column: "DefaultOptionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FilterMetas_FilterOptionMetas_DefaultOptionId",
                table: "FilterMetas",
                column: "DefaultOptionId",
                principalTable: "FilterOptionMetas",
                principalColumn: "Id");

            migrationBuilder.DropColumn(
                name: "AutoSelectLabel",
                table: "FilterMetas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FilterMetas_FilterOptionMetas_DefaultOptionId",
                table: "FilterMetas");

            migrationBuilder.DropIndex(
                name: "IX_FilterMetas_DefaultOptionId",
                table: "FilterMetas");

            migrationBuilder.DropColumn(
                name: "DefaultOptionId",
                table: "FilterMetas");

            migrationBuilder.AddColumn<string>(
                name: "AutoSelectLabel",
                table: "FilterMetas",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);
        }
    }
}
