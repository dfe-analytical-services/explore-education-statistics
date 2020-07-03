using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES931ChangeLaCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE dbo.Location SET LocalAuthority_Code = NULL WHERE LocalAuthority_Name = 'Bedfordshire (Pre LGR 2009)' AND LocalAuthority_OldCode = '820' AND LocalAuthority_Code = '.'");

            migrationBuilder.Sql(
                "UPDATE dbo.Location SET LocalAuthority_Code = NULL WHERE LocalAuthority_Name = 'Cheshire (Pre LGR 2009)' AND LocalAuthority_OldCode = '875' AND LocalAuthority_Code = '.'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE dbo.Location SET LocalAuthority_Code = '.' WHERE LocalAuthority_Name = 'Bedfordshire (Pre LGR 2009)' AND LocalAuthority_OldCode = '820' AND LocalAuthority_Code IS NULL");

            migrationBuilder.Sql(
                "UPDATE dbo.Location SET LocalAuthority_Code = '.' WHERE LocalAuthority_Name = 'Cheshire (Pre LGR 2009)' AND LocalAuthority_OldCode = '875' AND LocalAuthority_Code IS NULL");
        }
    }
}