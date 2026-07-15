using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class Ees7173GrantDataFactoryUserAbilityToExecuteStoredProcedures : Migration
    {
        private const string MigrationId = "20260709190717";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("GRANT EXECUTE ON OBJECT::RemoveSoftDeletedSubjects TO [datafactory]");
            migrationBuilder.Sql("GRANT EXECUTE ON OBJECT::RebuildIndexes TO [datafactory]");
            migrationBuilder.Sql("GRANT EXECUTE ON OBJECT::KillIndexReorganizations TO [datafactory]");
            migrationBuilder.Sql("GRANT EXECUTE ON OBJECT::PauseResumableIndexRebuilds TO [datafactory]");
            migrationBuilder.Sql("GRANT EXECUTE ON OBJECT::UpdateStatistics TO [datafactory]");
            migrationBuilder.Sql("GRANT EXECUTE ON OBJECT::UpdateStatistics TO [datafactory]");
            migrationBuilder.Sql("GRANT EXECUTE ON TYPE::ModifiedTablesType TO [datafactory]");

            migrationBuilder.Sql("GRANT VIEW DATABASE STATE TO [datafactory]");
            migrationBuilder.Sql("GRANT ALTER ON SCHEMA::[dbo] TO [datafactory]");
            migrationBuilder.Sql("GRANT VIEW DEFINITION TO [datafactory]");

            migrationBuilder.SqlFromFile(
                MigrationConstants.MigrationsPath,
                $"{MigrationId}_{nameof(Ees7173GrantDataFactoryUserAbilityToExecuteStoredProcedures)}.sql"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
