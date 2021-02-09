using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1836_RemoveObservationRowFilterItemIdentityId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE ObservationRowFilterItem DROP CONSTRAINT PK_ObservationRowFilterItem");
            migrationBuilder.Sql("ALTER TABLE ObservationRowFilterItem DROP COLUMN Id");
            migrationBuilder.Sql("ALTER TABLE ObservationRowFilterItem ADD CONSTRAINT PK_ObservationRowFilterItem PRIMARY KEY CLUSTERED (ObservationId, FilterItemId)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
