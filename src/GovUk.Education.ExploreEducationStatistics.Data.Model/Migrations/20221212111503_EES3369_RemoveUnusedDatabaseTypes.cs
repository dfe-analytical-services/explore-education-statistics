using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES3369_RemoveUnusedDatabaseTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TYPE FilterTableType");
            migrationBuilder.Sql("DROP TYPE FootnoteType");
            migrationBuilder.Sql("DROP TYPE IdListVarcharType");
            migrationBuilder.Sql("DROP TYPE IdListIntegerType");
            migrationBuilder.Sql("DROP TYPE TimePeriodListType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
