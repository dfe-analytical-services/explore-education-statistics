using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class RenameGeographicLevels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4a1af98a-ed8a-438e-92d4-d21cca0429f9"),
                column: "DataBlock_Request",
                value: "{\"SubjectId\":1,\"GeographicLevel\":\"LocalAuthorityDistrict\",\"TimePeriod\":{\"StartYear\":\"2016\",\"StartCode\":\"AY\",\"EndYear\":\"2017\",\"EndCode\":\"AY\"},\"Filters\":[\"1\",\"58\"],\"Indicators\":[\"23\",\"26\",\"28\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4a1af98a-ed8a-438e-92d4-d21cca0429f9"),
                column: "DataBlock_Request",
                value: "{\"SubjectId\":1,\"GeographicLevel\":\"Local_Authority_District\",\"TimePeriod\":{\"StartYear\":\"2016\",\"StartCode\":\"AY\",\"EndYear\":\"2017\",\"EndCode\":\"AY\"},\"Filters\":[\"1\",\"58\"],\"Indicators\":[\"23\",\"26\",\"28\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}");
        }
    }
}
