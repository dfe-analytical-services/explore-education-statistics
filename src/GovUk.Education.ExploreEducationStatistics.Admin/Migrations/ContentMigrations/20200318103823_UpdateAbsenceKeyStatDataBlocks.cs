using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class UpdateAbsenceKeyStatDataBlocks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("045a9585-688f-46fa-b3a9-9bdc237e0381"),
                columns: new[] { "DataBlock_Request", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"TimePeriod\":{\"StartYear\":2016,\"StartCode\":\"AY\",\"EndYear\":2016,\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"BoundaryLevel\":null,\"GeographicLevel\":null,\"Indicators\":[\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null,\"PlanningArea\":null,\"IncludeGeoJson\":true}", "{\"dataKeys\":[\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"],\"dataSummary\":[\"Up from 1.1% in 2015/16\"],\"dataDefinition\":[\"Number of unauthorised absences as a percentage of the overall school population.\"],\"dataDefinitionTitle\":[\"What is unauthorized absence rate?\"]}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Unauthorised absence rate\",\"value\":\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("3da30a08-9eeb-4a99-9872-796c3ea518fa"),
                columns: new[] { "DataBlock_Request", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"TimePeriod\":{\"StartYear\":2016,\"StartCode\":\"AY\",\"EndYear\":2016,\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"BoundaryLevel\":null,\"GeographicLevel\":null,\"Indicators\":[\"f9ae4976-7cd3-4718-834a-09349b6eb377\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null,\"PlanningArea\":null,\"IncludeGeoJson\":true}", "{\"dataKeys\":[\"f9ae4976-7cd3-4718-834a-09349b6eb377\"],\"dataSummary\":[\"Similar to previous years\"],\"dataDefinition\":[\"Number of authorised absences as a percentage of the overall school population.\"],\"dataDefinitionTitle\":[\"What is authorized absence rate?\"]}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Authorised absence rate\",\"value\":\"f9ae4976-7cd3-4718-834a-09349b6eb377\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("9ccb0daf-91a1-4cb0-b3c1-2aed452338bc"),
                columns: new[] { "DataBlock_Request", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"TimePeriod\":{\"StartYear\":2016,\"StartCode\":\"AY\",\"EndYear\":2016,\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"BoundaryLevel\":null,\"GeographicLevel\":null,\"Indicators\":[\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null,\"PlanningArea\":null,\"IncludeGeoJson\":true}", "{\"dataKeys\":[\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\"],\"dataSummary\":[\"Up from 4.6% in 2015/16\"],\"dataDefinition\":[\"Total number of all authorised and unauthorised absences from possible school sessions for all pupils.\"],\"dataDefinitionTitle\":[\"What is overall absence?\"]}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Overall absence rate\",\"value\":\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\"}]}}]" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("045a9585-688f-46fa-b3a9-9bdc237e0381"),
                columns: new[] { "DataBlock_Request", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"TimePeriod\":{\"StartYear\":2012,\"StartCode\":\"AY\",\"EndYear\":2016,\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"BoundaryLevel\":null,\"GeographicLevel\":null,\"Indicators\":[\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null,\"PlanningArea\":null,\"IncludeGeoJson\":true}", "{\"dataKeys\":[\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"],\"dataSummary\":[\"Up from 1.1% in 2015/16\"],\"dataDefinition\":[\"Number of unauthorised absences as a percentage of the overall school population. <a href=\\\"/glossary#unauthorised-absence\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is unauthorized absence rate?\"]}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Unauthorised absence rate\",\"value\":\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("3da30a08-9eeb-4a99-9872-796c3ea518fa"),
                columns: new[] { "DataBlock_Request", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"TimePeriod\":{\"StartYear\":2012,\"StartCode\":\"AY\",\"EndYear\":2016,\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"BoundaryLevel\":null,\"GeographicLevel\":null,\"Indicators\":[\"f9ae4976-7cd3-4718-834a-09349b6eb377\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null,\"PlanningArea\":null,\"IncludeGeoJson\":true}", "{\"dataKeys\":[\"f9ae4976-7cd3-4718-834a-09349b6eb377\"],\"dataSummary\":[\"Similar to previous years\"],\"dataDefinition\":[\"Number of authorised absences as a percentage of the overall school population. <a href=\\\"/glossary#authorised-absence\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is authorized absence rate?\"]}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Authorised absence rate\",\"value\":\"f9ae4976-7cd3-4718-834a-09349b6eb377\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("9ccb0daf-91a1-4cb0-b3c1-2aed452338bc"),
                columns: new[] { "DataBlock_Request", "DataBlock_Summary", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"TimePeriod\":{\"StartYear\":2012,\"StartCode\":\"AY\",\"EndYear\":2016,\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"BoundaryLevel\":null,\"GeographicLevel\":null,\"Indicators\":[\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null,\"PlanningArea\":null,\"IncludeGeoJson\":true}", "{\"dataKeys\":[\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\"],\"dataSummary\":[\"Up from 4.6% in 2015/16\"],\"dataDefinition\":[\"Total number of all authorised and unauthorised absences from possible school sessions for all pupils. <a href=\\\"/glossary#overall-absence\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is overall absence?\"]}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Overall absence rate\",\"value\":\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\"}]}}]" });
        }
    }
}
