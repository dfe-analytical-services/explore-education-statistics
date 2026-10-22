using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class Ees7768AddIdsToFootnoteLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IndicatorGroupId",
                table: "IndicatorFootnote",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddColumn<Guid>(
                name: "SubjectId",
                table: "IndicatorFootnote",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddColumn<Guid>(
                name: "FilterGroupId",
                table: "FilterItemFootnote",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddColumn<Guid>(
                name: "FilterId",
                table: "FilterItemFootnote",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddColumn<Guid>(
                name: "SubjectId",
                table: "FilterItemFootnote",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddColumn<Guid>(
                name: "FilterId",
                table: "FilterGroupFootnote",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddColumn<Guid>(
                name: "SubjectId",
                table: "FilterGroupFootnote",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddColumn<Guid>(
                name: "SubjectId",
                table: "FilterFootnote",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.Sql(
                @"
                UPDATE FilterFootnote
                SET SubjectId = Filter.SubjectId
                FROM FilterFootnote
                JOIN Filter ON Filter.Id = FilterFootnote.FilterId;

                UPDATE FilterGroupFootnote
                SET FilterId = Filter.Id,
                    SubjectId = Filter.SubjectId
                FROM FilterGroupFootnote
                JOIN FilterGroup ON FilterGroup.Id = FilterGroupFootnote.FilterGroupId
                JOIN Filter ON Filter.Id = FilterGroup.FilterId;

                UPDATE FilterItemFootnote
                SET FilterGroupId = FilterGroup.Id,
                    FilterId = Filter.Id,
                    SubjectId = Filter.SubjectId
                FROM FilterItemFootnote
                JOIN FilterItem ON FilterItem.Id = FilterItemFootnote.FilterItemId
                JOIN FilterGroup ON FilterGroup.Id = FilterItem.FilterGroupId
                JOIN Filter ON Filter.Id = FilterGroup.FilterId;

                UPDATE IndicatorFootnote
                SET IndicatorGroupId = IndicatorGroup.Id,
                    SubjectId = IndicatorGroup.SubjectId
                FROM IndicatorFootnote
                JOIN Indicator ON Indicator.Id = IndicatorFootnote.IndicatorId
                JOIN IndicatorGroup ON IndicatorGroup.Id = Indicator.IndicatorGroupId;
                "
            );

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorFootnote_SubjectId",
                table: "IndicatorFootnote",
                column: "SubjectId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_FilterItemFootnote_SubjectId",
                table: "FilterItemFootnote",
                column: "SubjectId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_FilterGroupFootnote_SubjectId",
                table: "FilterGroupFootnote",
                column: "SubjectId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_FilterFootnote_SubjectId",
                table: "FilterFootnote",
                column: "SubjectId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_IndicatorFootnote_SubjectId", table: "IndicatorFootnote");

            migrationBuilder.DropIndex(name: "IX_FilterItemFootnote_SubjectId", table: "FilterItemFootnote");

            migrationBuilder.DropIndex(name: "IX_FilterGroupFootnote_SubjectId", table: "FilterGroupFootnote");

            migrationBuilder.DropIndex(name: "IX_FilterFootnote_SubjectId", table: "FilterFootnote");

            migrationBuilder.DropColumn(name: "IndicatorGroupId", table: "IndicatorFootnote");

            migrationBuilder.DropColumn(name: "SubjectId", table: "IndicatorFootnote");

            migrationBuilder.DropColumn(name: "FilterGroupId", table: "FilterItemFootnote");

            migrationBuilder.DropColumn(name: "FilterId", table: "FilterItemFootnote");

            migrationBuilder.DropColumn(name: "SubjectId", table: "FilterItemFootnote");

            migrationBuilder.DropColumn(name: "FilterId", table: "FilterGroupFootnote");

            migrationBuilder.DropColumn(name: "SubjectId", table: "FilterGroupFootnote");

            migrationBuilder.DropColumn(name: "SubjectId", table: "FilterFootnote");
        }
    }
}
