using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class Ees6696Routine_PauseResumableIndexRebuilds : Migration
    {
        internal const string MigrationId = "20251112113245";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(
                MigrationConstants.MigrationsPath,
                $"{MigrationId}_Routine_PauseResumableIndexRebuilds.sql"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE PauseResumableIndexRebuilds");
        }
    }
}
