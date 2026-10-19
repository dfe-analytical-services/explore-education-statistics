using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class DropReleaseFootnoteReleaseVersionForeignKey : Migration
    {
        private const string PreviousRemoveSoftDeletedSubjectsMigrationId =
            EES6434_ImproveLoggingInRemoveSoftDeletedSubjectsSP.MigrationId;
        internal const string MigrationId = "20261019144822";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseFootnote_ReleaseVersion_ReleaseVersionId",
                table: "ReleaseFootnote"
            );

            migrationBuilder.SqlFromFile(
                MigrationConstants.MigrationsPath,
                $"{MigrationId}_Routine_RemoveSoftDeletedSubjects.sql"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(
                MigrationConstants.MigrationsPath,
                $"{PreviousRemoveSoftDeletedSubjectsMigrationId}_Routine_RemoveSoftDeletedSubjects.sql"
            );

            // ReleaseFootnotes created since Up ran have no statistics ReleaseVersion, so don't check existing rows.
            migrationBuilder.Sql(
                """
                ALTER TABLE ReleaseFootnote WITH NOCHECK
                ADD CONSTRAINT FK_ReleaseFootnote_ReleaseVersion_ReleaseVersionId
                FOREIGN KEY (ReleaseVersionId) REFERENCES ReleaseVersion (Id) ON DELETE CASCADE
                """
            );
        }
    }
}
