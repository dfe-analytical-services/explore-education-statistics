using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES3369_RemoveUnusedDataFactoryStoredProcedures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE DropAndCreateRelease");
            migrationBuilder.Sql("DROP PROCEDURE UpsertLocation");
            migrationBuilder.Sql("DROP TYPE LocationType");
            migrationBuilder.Sql("DROP TYPE ReleaseType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
