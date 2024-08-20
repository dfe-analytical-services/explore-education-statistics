using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class EES5405_AddDataSetVersionRelease : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_DataSetVersions_ReleaseFileId",
            table: "DataSetVersions");

        migrationBuilder.RenameColumn(
            name: "ReleaseFileId",
            table: "DataSetVersions",
            newName: "Release_ReleaseFileId");

        migrationBuilder.AddColumn<Guid>(
            name: "Release_DataSetFileId",
            table: "DataSetVersions",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<string>(
            name: "Release_Slug",
            table: "DataSetVersions",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "Release_Title",
            table: "DataSetVersions",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateIndex(
            name: "IX_DataSetVersions_Release_DataSetFileId",
            table: "DataSetVersions",
            column: "Release_DataSetFileId");

        migrationBuilder.CreateIndex(
            name: "IX_DataSetVersions_Release_ReleaseFileId",
            table: "DataSetVersions",
            column: "Release_ReleaseFileId",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_DataSetVersions_Release_DataSetFileId",
            table: "DataSetVersions");

        migrationBuilder.DropIndex(
            name: "IX_DataSetVersions_Release_ReleaseFileId",
            table: "DataSetVersions");

        migrationBuilder.DropColumn(
            name: "Release_DataSetFileId",
            table: "DataSetVersions");

        migrationBuilder.DropColumn(
            name: "Release_Slug",
            table: "DataSetVersions");

        migrationBuilder.DropColumn(
            name: "Release_Title",
            table: "DataSetVersions");

        migrationBuilder.RenameColumn(
            name: "Release_ReleaseFileId",
            table: "DataSetVersions",
            newName: "ReleaseFileId");

        migrationBuilder.CreateIndex(
            name: "IX_DataSetVersions_ReleaseFileId",
            table: "DataSetVersions",
            column: "ReleaseFileId");
    }
}
