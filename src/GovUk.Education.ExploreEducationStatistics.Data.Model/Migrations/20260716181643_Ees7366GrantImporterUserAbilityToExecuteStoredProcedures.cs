using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class Ees7366GrantImporterUserAbilityToExecuteStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("GRANT EXECUTE ON TYPE::ObservationType TO [importer]");
            migrationBuilder.Sql("GRANT EXECUTE ON TYPE::ObservationFilterItemType TO [importer]");
            migrationBuilder.Sql("GRANT EXECUTE ON OBJECT::InsertObservations TO [importer]");
            migrationBuilder.Sql("GRANT EXECUTE ON OBJECT::InsertObservationFilterItems TO [importer]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
