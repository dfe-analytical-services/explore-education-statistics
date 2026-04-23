using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    [ExcludeFromCodeCoverage]
    // ReSharper disable once InconsistentNaming
    /// <inheritdoc />
    public partial class EES6008_AddApiCompatibilityToReleaseFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PublicApiCompatible",
                table: "ReleaseFiles",
                type: "bit",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PublicApiCompatible", table: "ReleaseFiles");
        }
    }
}
