using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1500AddExperimentalAndTransparencyDataReleaseTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO ReleaseTypes (Id, Title) 
VALUES ('F5DE8522-3150-435D-98D5-1D14763F8C54', 'Experimental'),
       ('15BD4F57-C837-4821-B308-7F4169CD9330', 'Transparency Data');
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM ReleaseTypes 
WHERE Id = 'F5DE8522-3150-435D-98D5-1D14763F8C54'
OR Id = '15BD4F57-C837-4821-B308-7F4169CD9330';
");
        }
    }
}
