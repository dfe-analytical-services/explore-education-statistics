using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class EES4954_MovePublicIdToLocationMetaOptionLinkTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "LocationOptionMetaLinks",
                type: "text",
                nullable: true);
            
            migrationBuilder.CreateIndex(
                name: "IX_LocationOptionMetaLinks_PublicId",
                table: "LocationOptionMetaLinks",
                column: "PublicId");            
            
            migrationBuilder.CreateIndex(
                name: "IX_LocationOptionMetaLinks_MetaId_PublicId",
                table: "LocationOptionMetaLinks",
                columns: new[] { "MetaId", "PublicId" },
                unique: true);

            migrationBuilder.Sql(
                """
                UPDATE "LocationOptionMetaLinks"
                SET "PublicId" = (
                  SELECT "PublicId" 
                  FROM "LocationOptionMetas" 
                  WHERE "LocationOptionMetas"."Id" = "LocationOptionMetaLinks"."OptionId"
                )
                """);
            
            migrationBuilder.AlterColumn<Guid>(
                name: "PublicId",
                table: "LocationOptionMetaLinks",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
            
            migrationBuilder.DropIndex(
                name: "IX_LocationOptionMetas_PublicId",
                table: "LocationOptionMetas");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "LocationOptionMetas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "LocationOptionMetaLinks");            
            
            migrationBuilder.DropIndex(
                name: "IX_LocationOptionMetaLinks_MetaId_PublicId",
                table: "LocationOptionMetaLinks");

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "LocationOptionMetas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_LocationOptionMetas_PublicId",
                table: "LocationOptionMetas",
                column: "PublicId",
                unique: true);
        }
    }
}
