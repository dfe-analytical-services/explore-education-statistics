using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations;

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
            column: "DefaultOptionId");

        migrationBuilder.AddForeignKey(
            name: "FK_FilterMetas_FilterOptionMetas_DefaultOptionId",
            table: "FilterMetas",
            column: "DefaultOptionId",
            principalTable: "FilterOptionMetas",
            principalColumn: "Id");

        migrationBuilder.Sql(
            """
            UPDATE "FilterMetas" FM
            SET "DefaultOptionId" = (
                SELECT FOM."Id"
                FROM "FilterOptionMetas" FOM
                JOIN "FilterOptionMetaLinks" FOML on FOM."Id" = FOML."OptionId" AND FOML."MetaId" = FM."Id"
                WHERE FOM."Label" = FM."AutoSelectLabel"
            )
            WHERE FM."AutoSelectLabel" IS NOT NULL;                 
            """);


        migrationBuilder.DropColumn(
            name: "AutoSelectLabel",
            table: "FilterMetas");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "AutoSelectLabel",
            table: "FilterMetas",
            type: "character varying(120)",
            maxLength: 120,
            nullable: true);

        migrationBuilder.Sql(
            """
            UPDATE "FilterMetas" FM
            SET "AutoSelectLabel" = (
                SELECT FOM."Label"
                FROM "FilterOptionMetas" FOM
                JOIN "FilterOptionMetaLinks" FOML on FOM."Id" = FOML."OptionId" AND FOML."MetaId" = FM."Id"
                WHERE FOM."Id" = FM."DefaultOptionId"
            )
            WHERE FM."DefaultOptionId" IS NOT NULL;
            """);

        migrationBuilder.DropForeignKey(
            name: "FK_FilterMetas_FilterOptionMetas_DefaultOptionId",
            table: "FilterMetas");

        migrationBuilder.DropIndex(
            name: "IX_FilterMetas_DefaultOptionId",
            table: "FilterMetas");

        migrationBuilder.DropColumn(
            name: "DefaultOptionId",
            table: "FilterMetas");
    }
}
