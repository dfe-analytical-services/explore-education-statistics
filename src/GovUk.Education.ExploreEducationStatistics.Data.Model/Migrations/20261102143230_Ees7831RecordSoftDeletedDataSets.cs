using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class Ees7831RecordSoftDeletedDataSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SoftDeletedDataStorageVersion",
                table: "Subject",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(
                name: "SoftDeletedReleaseVersionId",
                table: "Subject",
                type: "uniqueidentifier",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "SoftDeletedDataStorageVersion", table: "Subject");

            migrationBuilder.DropColumn(name: "SoftDeletedReleaseVersionId", table: "Subject");
        }
    }
}
