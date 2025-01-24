using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class EES4735_RenameIsAggregateToIsAutoSelect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AutoSelectLabel",
                table: "FilterMetas",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.Sql("""
                                 UPDATE "FilterMetas" FM
                                 SET "AutoSelectLabel" = 'Total'
                                 WHERE FM."AutoSelectLabel" IS NULL
                                 AND EXISTS (
                                     SELECT 1
                                     FROM "FilterOptionMetas" FOM
                                     JOIN "FilterOptionMetaLinks" FOML ON FM."Id" = FOML."MetaId"
                                     WHERE FOML."OptionId" = FOM."Id" AND FOM."Label" = 'Total');
                                 """);

            migrationBuilder.DropColumn(
                name: "IsAggregate",
                table: "FilterOptionMetas");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoSelectLabel",
                table: "FilterMetas");

            migrationBuilder.AddColumn<bool>(
                name: "IsAggregate",
                table: "FilterOptionMetas",
                type: "boolean",
                nullable: true);
        }
    }
}
