using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class EES5999_AddFileSizePropertiesToUploads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DataFileSizeInBytes",
                table: "DataSetUploads",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "MetaFileSizeInBytes",
                table: "DataSetUploads",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "ReplacingFileId",
                table: "DataSetUploads",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataFileSizeInBytes",
                table: "DataSetUploads");

            migrationBuilder.DropColumn(
                name: "MetaFileSizeInBytes",
                table: "DataSetUploads");

            migrationBuilder.DropColumn(
                name: "ReplacingFileId",
                table: "DataSetUploads");
        }
    }
}
