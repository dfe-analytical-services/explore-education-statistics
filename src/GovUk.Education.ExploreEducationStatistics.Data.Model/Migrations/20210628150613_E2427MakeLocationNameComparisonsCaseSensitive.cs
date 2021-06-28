using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class E2427MakeLocationNameComparisonsCaseSensitive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_Location_RscRegion_Code", "Location");
            migrationBuilder.Sql(@"
                ALTER TABLE Location ALTER COLUMN Country_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CS_AS;
                ALTER TABLE Location ALTER COLUMN Institution_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CS_AS;
                ALTER TABLE Location ALTER COLUMN LocalAuthority_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CS_AS;
                ALTER TABLE Location ALTER COLUMN LocalAuthorityDistrict_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CS_AS;
                ALTER TABLE Location ALTER COLUMN LocalEnterprisePartnership_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CS_AS;
                ALTER TABLE Location ALTER COLUMN MayoralCombinedAuthority_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CS_AS;
                ALTER TABLE Location ALTER COLUMN MultiAcademyTrust_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CS_AS;
                ALTER TABLE Location ALTER COLUMN OpportunityArea_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CS_AS;
                ALTER TABLE Location ALTER COLUMN ParliamentaryConstituency_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CS_AS;
                ALTER TABLE Location ALTER COLUMN Region_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CS_AS;
                ALTER TABLE Location ALTER COLUMN RscRegion_Code NVARCHAR(450) COLLATE SQL_Latin1_General_CP1_CS_AS;
                ALTER TABLE Location ALTER COLUMN Sponsor_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CS_AS;
                ALTER TABLE Location ALTER COLUMN Ward_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CS_AS;
                ALTER TABLE Location ALTER COLUMN PlanningArea_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CS_AS;
                ALTER TABLE Location ALTER COLUMN EnglishDevolvedArea_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CS_AS;
            ");
            migrationBuilder.CreateIndex("IX_Location_RscRegion_Code", "Location", "RscRegion_Code");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_Location_RscRegion_Code", "Location");
            migrationBuilder.Sql(@"
                ALTER TABLE Location ALTER COLUMN Country_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS;
                ALTER TABLE Location ALTER COLUMN Institution_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS;
                ALTER TABLE Location ALTER COLUMN LocalAuthority_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS;
                ALTER TABLE Location ALTER COLUMN LocalAuthorityDistrict_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS;
                ALTER TABLE Location ALTER COLUMN LocalEnterprisePartnership_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS;
                ALTER TABLE Location ALTER COLUMN MayoralCombinedAuthority_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS;
                ALTER TABLE Location ALTER COLUMN MultiAcademyTrust_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS;
                ALTER TABLE Location ALTER COLUMN OpportunityArea_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS;
                ALTER TABLE Location ALTER COLUMN ParliamentaryConstituency_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS;
                ALTER TABLE Location ALTER COLUMN Region_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS;
                ALTER TABLE Location ALTER COLUMN RscRegion_Code NVARCHAR(450) COLLATE SQL_Latin1_General_CP1_CI_AS;
                ALTER TABLE Location ALTER COLUMN Sponsor_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS;
                ALTER TABLE Location ALTER COLUMN Ward_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS;
                ALTER TABLE Location ALTER COLUMN PlanningArea_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS;
                ALTER TABLE Location ALTER COLUMN EnglishDevolvedArea_Name NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS;
            ");
            migrationBuilder.CreateIndex("IX_Location_RscRegion_Code", "Location", "RscRegion_Code");
        }
    }
}
