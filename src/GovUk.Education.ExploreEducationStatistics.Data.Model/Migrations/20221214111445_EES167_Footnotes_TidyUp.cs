using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

[ExcludeFromCodeCoverage]
public partial class EES167_Footnotes_TidyUp : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Make Footnote Content column not nullable
        migrationBuilder.AlterColumn<string>(
            name: "Content",
            table: "Footnote",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        // Remove foreign keys with FootnoteId from FilterFootnote, FilterGroupFootnote, FilterItemFootnote
        // IndicatorFootnote, and SubjectFootnote tables

        migrationBuilder.DropForeignKey(
            name: "FK_FilterFootnote_Footnote_FootnoteId",
            table: "FilterFootnote");

        migrationBuilder.DropForeignKey(
            name: "FK_FilterGroupFootnote_Footnote_FootnoteId",
            table: "FilterGroupFootnote");

        migrationBuilder.DropForeignKey(
            name: "FK_FilterItemFootnote_Footnote_FootnoteId",
            table: "FilterItemFootnote");

        migrationBuilder.DropForeignKey(
            name: "FK_IndicatorFootnote_Footnote_FootnoteId",
            table: "IndicatorFootnote");

        migrationBuilder.DropForeignKey(
            name: "FK_SubjectFootnote_Footnote_FootnoteId",
            table: "SubjectFootnote");

        // Recreate the same foreign keys but this time with a cascade delete action

        migrationBuilder.AddForeignKey(
            name: "FK_FilterFootnote_Footnote_FootnoteId",
            table: "FilterFootnote",
            column: "FootnoteId",
            principalTable: "Footnote",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_FilterGroupFootnote_Footnote_FootnoteId",
            table: "FilterGroupFootnote",
            column: "FootnoteId",
            principalTable: "Footnote",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_FilterItemFootnote_Footnote_FootnoteId",
            table: "FilterItemFootnote",
            column: "FootnoteId",
            principalTable: "Footnote",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_IndicatorFootnote_Footnote_FootnoteId",
            table: "IndicatorFootnote",
            column: "FootnoteId",
            principalTable: "Footnote",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_SubjectFootnote_Footnote_FootnoteId",
            table: "SubjectFootnote",
            column: "FootnoteId",
            principalTable: "Footnote",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Content",
            table: "Footnote",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.DropForeignKey(
            name: "FK_FilterFootnote_Footnote_FootnoteId",
            table: "FilterFootnote");

        migrationBuilder.DropForeignKey(
            name: "FK_FilterGroupFootnote_Footnote_FootnoteId",
            table: "FilterGroupFootnote");

        migrationBuilder.DropForeignKey(
            name: "FK_FilterItemFootnote_Footnote_FootnoteId",
            table: "FilterItemFootnote");

        migrationBuilder.DropForeignKey(
            name: "FK_IndicatorFootnote_Footnote_FootnoteId",
            table: "IndicatorFootnote");

        migrationBuilder.DropForeignKey(
            name: "FK_SubjectFootnote_Footnote_FootnoteId",
            table: "SubjectFootnote");

        migrationBuilder.AddForeignKey(
            name: "FK_FilterFootnote_Footnote_FootnoteId",
            table: "FilterFootnote",
            column: "FootnoteId",
            principalTable: "Footnote",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_FilterGroupFootnote_Footnote_FootnoteId",
            table: "FilterGroupFootnote",
            column: "FootnoteId",
            principalTable: "Footnote",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_FilterItemFootnote_Footnote_FootnoteId",
            table: "FilterItemFootnote",
            column: "FootnoteId",
            principalTable: "Footnote",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_IndicatorFootnote_Footnote_FootnoteId",
            table: "IndicatorFootnote",
            column: "FootnoteId",
            principalTable: "Footnote",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_SubjectFootnote_Footnote_FootnoteId",
            table: "SubjectFootnote",
            column: "FootnoteId",
            principalTable: "Footnote",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}
