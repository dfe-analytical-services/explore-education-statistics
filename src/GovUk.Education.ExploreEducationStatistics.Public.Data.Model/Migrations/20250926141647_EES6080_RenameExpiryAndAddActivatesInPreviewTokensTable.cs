using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class EES6080_RenameExpiryAndAddActivatesInPreviewTokensTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Expiry",
                table: "PreviewTokens",
                newName: "Expires");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Activates",
                table: "PreviewTokens",
                type: "timestamp with time zone",
                nullable: true);
            
            migrationBuilder.Sql(
                @"UPDATE ""PreviewTokens"" SET ""Activates"" = ""Created"";");
            
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Activates",
                table: "PreviewTokens",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activates",
                table: "PreviewTokens");

            migrationBuilder.RenameColumn(
                name: "Expires",
                table: "PreviewTokens",
                newName: "Expiry");
        }
    }
}
