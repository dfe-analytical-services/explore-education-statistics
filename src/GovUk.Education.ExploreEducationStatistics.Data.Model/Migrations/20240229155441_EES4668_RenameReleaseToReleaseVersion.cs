using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

[ExcludeFromCodeCoverage]
public partial class EES4668_RenameReleaseToReleaseVersion : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // As well as renaming the Release table, 2 foreign keys referencing ReleaseId need to be renamed.
        // They are:
        // - ReleaseFootnote.ReleaseId
        // - ReleaseSubject.ReleaseId

        // Drop the 2 foreign key constraints
        migrationBuilder.DropForeignKey(
            name: "FK_ReleaseFootnote_Release_ReleaseId",
            table: "ReleaseFootnote"
        );

        migrationBuilder.DropForeignKey(
            name: "FK_ReleaseSubject_Release_ReleaseId",
            table: "ReleaseSubject"
        );

        // Rename the Release table to ReleaseVersion
        migrationBuilder.RenameTable(name: "Release", newName: "ReleaseVersion");

        // Rename the ReleaseId column to ReleaseVersionId in the 2 foreign key columns
        migrationBuilder.RenameColumn(
            name: "ReleaseId",
            newName: "ReleaseVersionId",
            table: "ReleaseFootnote"
        );

        migrationBuilder.RenameColumn(
            name: "ReleaseId",
            newName: "ReleaseVersionId",
            table: "ReleaseSubject"
        );

        // Note, there's no indexes IX_ReleaseFootnote_ReleaseId or IX_ReleaseSubject_ReleaseId
        // so there's no renaming of any indexes here.

        // Recreate the 2 foreign key constraints
        migrationBuilder.AddForeignKey(
            name: "FK_ReleaseFootnote_ReleaseVersion_ReleaseVersionId",
            table: "ReleaseFootnote",
            column: "ReleaseVersionId",
            principalTable: "ReleaseVersion",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
        );

        migrationBuilder.AddForeignKey(
            name: "FK_ReleaseSubject_ReleaseVersion_ReleaseVersionId",
            table: "ReleaseSubject",
            column: "ReleaseVersionId",
            principalTable: "ReleaseVersion",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder) { }
}
