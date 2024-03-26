using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

public partial class EES4668_RenameReleaseVersionIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Rename ReleaseVersion indexes which were not renamed by the Release -> ReleaseVersion table rename
        // in migration 20240229155441_EES4668_RenameReleaseToReleaseVersion
        migrationBuilder.RenameIndex(
            name: "PK_Release",
            table: "ReleaseVersion",
            newName: "PK_ReleaseVersion");

        migrationBuilder.RenameIndex(
            name: "IX_Release_PublicationId",
            table: "ReleaseVersion",
            newName: "IX_ReleaseVersion_PublicationId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
