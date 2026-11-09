using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class Ees7818AddFootnoteRolloverFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RolloverHasChanges",
                table: "Footnote",
                type: "bit",
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(
                name: "RolloverSourceFootnoteId",
                table: "Footnote",
                type: "uniqueidentifier",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "RolloverHasChanges", table: "Footnote");

            migrationBuilder.DropColumn(name: "RolloverSourceFootnoteId", table: "Footnote");
        }
    }
}
