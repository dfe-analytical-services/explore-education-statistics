using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class CorrectDataBlockTableHeaders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("02a637e7-6cc7-44e5-8991-8982edfe49fc"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"fa0d7f1d-d181-43fb-955b-fc327da86f2c\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"e957db0c-3bf8-4e4b-5c6f-08d78f900a85\"],\"Indicators\":[\"94f9b11c-df82-4eef-4c29-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"tableHeaders\":{\"columnGroups\":[[{\"label\":\"All primary\",\"value\":\"e957db0c-3bf8-4e4b-5c6f-08d78f900a85\"}]],\"columns\":[{\"label\":\"2014\",\"value\":\"2014_CY\"},{\"label\":\"2015\",\"value\":\"2015_CY\"},{\"label\":\"2016\",\"value\":\"2016_CY\"},{\"label\":\"2017\",\"value\":\"2017_CY\"},{\"label\":\"2018\",\"value\":\"2018_CY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Number of first preferences offered\",\"value\":\"94f9b11c-df82-4eef-4c29-08d78f90080f\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("038093a2-0be3-440b-8b22-8116e34aa616"),
                columns: new[] { "DataBlock_Request", "Name", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"Indicators\":[\"68aeda43-2b6a-433a-4c06-08d78f90080f\",\"a5a58f92-aba1-4955-4c02-08d78f90080f\",\"f045bc8d-8dd1-4f16-4c05-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "Generic data block 2", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Number of pupils\",\"value\":\"a5a58f92-aba1-4955-4c02-08d78f90080f\"},{\"label\":\"Number of fixed period exclusions\",\"value\":\"f045bc8d-8dd1-4f16-4c05-08d78f90080f\"},{\"label\":\"Fixed period exclusion rate\",\"value\":\"68aeda43-2b6a-433a-4c06-08d78f90080f\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("045a9585-688f-46fa-b3a9-9bdc237e0381"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Indicators\":[\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Unauthorised absence rate\",\"value\":\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("0b4c43cd-fc12-4159-88b9-0c8646424555"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"Indicators\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Number of permanent exclusions\",\"value\":\"167f4807-4fdd-461a-4c03-08d78f90080f\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17251e1c-e978-419c-98f5-963131c952f7"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"Indicators\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Number of permanent exclusions\",\"value\":\"167f4807-4fdd-461a-4c03-08d78f90080f\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"Indicators\":[\"b3df4fb1-dae3-4c16-4c01-08d78f90080f\",\"a5a58f92-aba1-4955-4c02-08d78f90080f\",\"167f4807-4fdd-461a-4c03-08d78f90080f\",\"be3b765b-005f-4279-4c04-08d78f90080f\",\"f045bc8d-8dd1-4f16-4c05-08d78f90080f\",\"68aeda43-2b6a-433a-4c06-08d78f90080f\",\"732f0d7b-dcd3-4bf8-4c08-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Permanent exclusion rate\",\"value\":\"be3b765b-005f-4279-4c04-08d78f90080f\"},{\"label\":\"Fixed period exclusion rate\",\"value\":\"68aeda43-2b6a-433a-4c06-08d78f90080f\"},{\"label\":\"Number of permanent exclusions\",\"value\":\"167f4807-4fdd-461a-4c03-08d78f90080f\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("1869d10a-ca3f-450c-9685-780b11d916f5"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"Indicators\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Number of permanent exclusions\",\"value\":\"167f4807-4fdd-461a-4c03-08d78f90080f\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("3da30a08-9eeb-4a99-9872-796c3ea518fa"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Indicators\":[\"f9ae4976-7cd3-4718-834a-09349b6eb377\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Authorised absence rate\",\"value\":\"f9ae4976-7cd3-4718-834a-09349b6eb377\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"fa0d7f1d-d181-43fb-955b-fc327da86f2c\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"e957db0c-3bf8-4e4b-5c6f-08d78f900a85\"],\"Indicators\":[\"49d2a1f4-e4a9-4f25-4c24-08d78f90080f\",\"020a4da6-1111-443d-af80-3a425c558d14\",\"94f9b11c-df82-4eef-4c29-08d78f90080f\",\"d22e1104-de56-4617-4c2a-08d78f90080f\",\"319dd956-a714-40fd-4c2b-08d78f90080f\",\"a9211c9d-b467-48d7-4c2c-08d78f90080f\",\"be1e1643-f7c8-40b0-4c2d-08d78f90080f\",\"16cdfc0a-f66f-496b-4c2e-08d78f90080f\",\"2c63589e-b5d4-4922-4c2f-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"tableHeaders\":{\"columnGroups\":[[{\"label\":\"All primary\",\"value\":\"e957db0c-3bf8-4e4b-5c6f-08d78f900a85\"}]],\"columns\":[{\"label\":\"2014\",\"value\":\"2014_CY\"},{\"label\":\"2015\",\"value\":\"2015_CY\"},{\"label\":\"2016\",\"value\":\"2016_CY\"},{\"label\":\"2017\",\"value\":\"2017_CY\"},{\"label\":\"2018\",\"value\":\"2018_CY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Number of applications received\",\"value\":\"020a4da6-1111-443d-af80-3a425c558d14\"},{\"label\":\"Number of admissions\",\"value\":\"49d2a1f4-e4a9-4f25-4c24-08d78f90080f\"},{\"label\":\"Number of first preferences offered\",\"value\":\"94f9b11c-df82-4eef-4c29-08d78f90080f\"},{\"label\":\"Number of second preferences offered\",\"value\":\"d22e1104-de56-4617-4c2a-08d78f90080f\"},{\"label\":\"Number of third preferences offered\",\"value\":\"319dd956-a714-40fd-4c2b-08d78f90080f\"},{\"label\":\"Number that received an offer for a non preferred school\",\"value\":\"16cdfc0a-f66f-496b-4c2e-08d78f90080f\"},{\"label\":\"Number that did not receive an offer\",\"value\":\"2c63589e-b5d4-4922-4c2f-08d78f90080f\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4a1af98a-ed8a-438e-92d4-d21cca0429f9"),
                column: "Name",
                value: "Generic data block - LA");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("52916052-81e3-4b66-80b8-24f8666d9cbf"),
                columns: new[] { "DataBlock_Request", "Name", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"fa0d7f1d-d181-43fb-955b-fc327da86f2c\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"5a7b4e97-7794-4037-5c71-08d78f900a85\"],\"Indicators\":[\"be1e1643-f7c8-40b0-4c2d-08d78f90080f\",\"16cdfc0a-f66f-496b-4c2e-08d78f90080f\",\"2c63589e-b5d4-4922-4c2f-08d78f90080f\",\"d10d4f10-c2f8-4120-4c30-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "Generic data block 1", "[{\"tableHeaders\":{\"columnGroups\":[[{\"label\":\"All secondary\",\"value\":\"5a7b4e97-7794-4037-5c71-08d78f900a85\"}]],\"columns\":[{\"label\":\"2014\",\"value\":\"2014_CY\"},{\"label\":\"2015\",\"value\":\"2015_CY\"},{\"label\":\"2016\",\"value\":\"2016_CY\"},{\"label\":\"2017\",\"value\":\"2017_CY\"},{\"label\":\"2018\",\"value\":\"2018_CY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Number that received an offer for a preferred school\",\"value\":\"be1e1643-f7c8-40b0-4c2d-08d78f90080f\"},{\"label\":\"Number that received an offer for a non preferred school\",\"value\":\"16cdfc0a-f66f-496b-4c2e-08d78f90080f\"},{\"label\":\"Number that did not receive an offer\",\"value\":\"2c63589e-b5d4-4922-4c2f-08d78f90080f\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5947759d-c6f3-451b-b353-a4da063f020a"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"fa0d7f1d-d181-43fb-955b-fc327da86f2c\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"e957db0c-3bf8-4e4b-5c6f-08d78f900a85\"],\"Indicators\":[\"020a4da6-1111-443d-af80-3a425c558d14\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"tableHeaders\":{\"columnGroups\":[[{\"label\":\"All primary\",\"value\":\"e957db0c-3bf8-4e4b-5c6f-08d78f900a85\"}]],\"columns\":[{\"label\":\"2014\",\"value\":\"2014_CY\"},{\"label\":\"2015\",\"value\":\"2015_CY\"},{\"label\":\"2016\",\"value\":\"2016_CY\"},{\"label\":\"2017\",\"value\":\"2017_CY\"},{\"label\":\"2018\",\"value\":\"2018_CY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Number of applications received\",\"value\":\"020a4da6-1111-443d-af80-3a425c558d14\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Indicators\":[\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"f9ae4976-7cd3-4718-834a-09349b6eb377\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Authorised absence rate\",\"value\":\"f9ae4976-7cd3-4718-834a-09349b6eb377\"},{\"label\":\"Unauthorised absence rate\",\"value\":\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"},{\"label\":\"Overall absence rate\",\"value\":\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"),
                columns: new[] { "DataBlock_Request", "Name", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Indicators\":[\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"f9ae4976-7cd3-4718-834a-09349b6eb377\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "Generic data block - National", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Authorised absence rate\",\"value\":\"f9ae4976-7cd3-4718-834a-09349b6eb377\"},{\"label\":\"Unauthorised absence rate\",\"value\":\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"},{\"label\":\"Overall absence rate\",\"value\":\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d5f9b1f-8d0d-47d4-ba2b-ea97413d3117"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"fa0d7f1d-d181-43fb-955b-fc327da86f2c\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"e957db0c-3bf8-4e4b-5c6f-08d78f900a85\"],\"Indicators\":[\"d22e1104-de56-4617-4c2a-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"tableHeaders\":{\"columnGroups\":[[{\"label\":\"All primary\",\"value\":\"e957db0c-3bf8-4e4b-5c6f-08d78f900a85\"}]],\"columns\":[{\"label\":\"2014\",\"value\":\"2014_CY\"},{\"label\":\"2015\",\"value\":\"2015_CY\"},{\"label\":\"2016\",\"value\":\"2016_CY\"},{\"label\":\"2017\",\"value\":\"2017_CY\"},{\"label\":\"2018\",\"value\":\"2018_CY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Number of second preferences offered\",\"value\":\"d22e1104-de56-4617-4c2a-08d78f90080f\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("695de169-947f-4f66-8564-6392b6113dfc"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"Indicators\":[\"68aeda43-2b6a-433a-4c06-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Fixed period exclusion rate\",\"value\":\"68aeda43-2b6a-433a-4c06-08d78f90080f\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("9ccb0daf-91a1-4cb0-b3c1-2aed452338bc"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Indicators\":[\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Overall absence rate\",\"value\":\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("a8c408ed-45d8-4690-a9f3-2fb0e86377bf"),
                columns: new[] { "DataBlock_Request", "Name", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"fa0d7f1d-d181-43fb-955b-fc327da86f2c\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"e957db0c-3bf8-4e4b-5c6f-08d78f900a85\"],\"Indicators\":[\"be1e1643-f7c8-40b0-4c2d-08d78f90080f\",\"16cdfc0a-f66f-496b-4c2e-08d78f90080f\",\"2c63589e-b5d4-4922-4c2f-08d78f90080f\",\"d10d4f10-c2f8-4120-4c30-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "Generic data block 2", "[{\"tableHeaders\":{\"columnGroups\":[[{\"label\":\"All primary\",\"value\":\"e957db0c-3bf8-4e4b-5c6f-08d78f900a85\"}]],\"columns\":[{\"label\":\"2014\",\"value\":\"2014_CY\"},{\"label\":\"2015\",\"value\":\"2015_CY\"},{\"label\":\"2016\",\"value\":\"2016_CY\"},{\"label\":\"2017\",\"value\":\"2017_CY\"},{\"label\":\"2018\",\"value\":\"2018_CY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Number that received an offer for a preferred school\",\"value\":\"be1e1643-f7c8-40b0-4c2d-08d78f90080f\"},{\"label\":\"Number that received an offer for a non preferred school\",\"value\":\"16cdfc0a-f66f-496b-4c2e-08d78f90080f\"},{\"label\":\"Number that did not receive an offer\",\"value\":\"2c63589e-b5d4-4922-4c2f-08d78f90080f\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("d0397918-1697-40d8-b649-bea3c63c7d3e"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"Indicators\":[\"be3b765b-005f-4279-4c04-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Permanent exclusion rate\",\"value\":\"be3b765b-005f-4279-4c04-08d78f90080f\"}]}}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("dd572e49-87e3-46f5-bb04-e9008573fc91"),
                columns: new[] { "DataBlock_Request", "Name", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"GeographicLevel\":null,\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"Indicators\":[\"be3b765b-005f-4279-4c04-08d78f90080f\",\"a5a58f92-aba1-4955-4c02-08d78f90080f\",\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "Generic data block 1", "[{\"tableHeaders\":{\"columnGroups\":[],\"columns\":[{\"label\":\"2012/13\",\"value\":\"2012_AY\"},{\"label\":\"2013/14\",\"value\":\"2013_AY\"},{\"label\":\"2014/15\",\"value\":\"2014_AY\"},{\"label\":\"2015/16\",\"value\":\"2015_AY\"},{\"label\":\"2016/17\",\"value\":\"2016_AY\"}],\"rowGroups\":[[{\"label\":\"England\",\"level\":\"country\",\"value\":\"E92000001\"}]],\"rows\":[{\"label\":\"Number of pupils\",\"value\":\"a5a58f92-aba1-4955-4c02-08d78f90080f\"},{\"label\":\"Number of permanent exclusions\",\"value\":\"167f4807-4fdd-461a-4c03-08d78f90080f\"},{\"label\":\"Permanent exclusion rate\",\"value\":\"be3b765b-005f-4279-4c04-08d78f90080f\"}]}}]" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("02a637e7-6cc7-44e5-8991-8982edfe49fc"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"fa0d7f1d-d181-43fb-955b-fc327da86f2c\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"e957db0c-3bf8-4e4b-5c6f-08d78f900a85\"],\"Indicators\":[\"94f9b11c-df82-4eef-4c29-08d78f90080f\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"indicators\":[\"94f9b11c-df82-4eef-4c29-08d78f90080f\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("038093a2-0be3-440b-8b22-8116e34aa616"),
                columns: new[] { "DataBlock_Request", "Name", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"Indicators\":[\"68aeda43-2b6a-433a-4c06-08d78f90080f\",\"a5a58f92-aba1-4955-4c02-08d78f90080f\",\"f045bc8d-8dd1-4f16-4c05-08d78f90080f\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, "[{\"indicators\":[\"a5a58f92-aba1-4955-4c02-08d78f90080f\",\"f045bc8d-8dd1-4f16-4c05-08d78f90080f\",\"68aeda43-2b6a-433a-4c06-08d78f90080f\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("045a9585-688f-46fa-b3a9-9bdc237e0381"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Indicators\":[\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"indicators\":[\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("0b4c43cd-fc12-4159-88b9-0c8646424555"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"Indicators\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"indicators\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17251e1c-e978-419c-98f5-963131c952f7"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"Indicators\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"indicators\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"Indicators\":[\"b3df4fb1-dae3-4c16-4c01-08d78f90080f\",\"a5a58f92-aba1-4955-4c02-08d78f90080f\",\"167f4807-4fdd-461a-4c03-08d78f90080f\",\"be3b765b-005f-4279-4c04-08d78f90080f\",\"f045bc8d-8dd1-4f16-4c05-08d78f90080f\",\"68aeda43-2b6a-433a-4c06-08d78f90080f\",\"732f0d7b-dcd3-4bf8-4c08-08d78f90080f\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"indicators\":[\"be3b765b-005f-4279-4c04-08d78f90080f\",\"68aeda43-2b6a-433a-4c06-08d78f90080f\",\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("1869d10a-ca3f-450c-9685-780b11d916f5"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"Indicators\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"indicators\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("3da30a08-9eeb-4a99-9872-796c3ea518fa"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Indicators\":[\"f9ae4976-7cd3-4718-834a-09349b6eb377\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"indicators\":[\"f9ae4976-7cd3-4718-834a-09349b6eb377\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"fa0d7f1d-d181-43fb-955b-fc327da86f2c\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"e957db0c-3bf8-4e4b-5c6f-08d78f900a85\"],\"Indicators\":[\"49d2a1f4-e4a9-4f25-4c24-08d78f90080f\",\"020a4da6-1111-443d-af80-3a425c558d14\",\"94f9b11c-df82-4eef-4c29-08d78f90080f\",\"d22e1104-de56-4617-4c2a-08d78f90080f\",\"319dd956-a714-40fd-4c2b-08d78f90080f\",\"a9211c9d-b467-48d7-4c2c-08d78f90080f\",\"be1e1643-f7c8-40b0-4c2d-08d78f90080f\",\"16cdfc0a-f66f-496b-4c2e-08d78f90080f\",\"2c63589e-b5d4-4922-4c2f-08d78f90080f\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"indicators\":[\"020a4da6-1111-443d-af80-3a425c558d14\",\"49d2a1f4-e4a9-4f25-4c24-08d78f90080f\",\"94f9b11c-df82-4eef-4c29-08d78f90080f\",\"d22e1104-de56-4617-4c2a-08d78f90080f\",\"319dd956-a714-40fd-4c2b-08d78f90080f\",\"16cdfc0a-f66f-496b-4c2e-08d78f90080f\",\"2c63589e-b5d4-4922-4c2f-08d78f90080f\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4a1af98a-ed8a-438e-92d4-d21cca0429f9"),
                column: "Name",
                value: null);

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("52916052-81e3-4b66-80b8-24f8666d9cbf"),
                columns: new[] { "DataBlock_Request", "Name", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"fa0d7f1d-d181-43fb-955b-fc327da86f2c\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"5a7b4e97-7794-4037-5c71-08d78f900a85\"],\"Indicators\":[\"be1e1643-f7c8-40b0-4c2d-08d78f90080f\",\"16cdfc0a-f66f-496b-4c2e-08d78f90080f\",\"2c63589e-b5d4-4922-4c2f-08d78f90080f\",\"d10d4f10-c2f8-4120-4c30-08d78f90080f\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, "[{\"indicators\":[\"be1e1643-f7c8-40b0-4c2d-08d78f90080f\",\"16cdfc0a-f66f-496b-4c2e-08d78f90080f\",\"2c63589e-b5d4-4922-4c2f-08d78f90080f\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5947759d-c6f3-451b-b353-a4da063f020a"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"fa0d7f1d-d181-43fb-955b-fc327da86f2c\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"e957db0c-3bf8-4e4b-5c6f-08d78f900a85\"],\"Indicators\":[\"020a4da6-1111-443d-af80-3a425c558d14\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"indicators\":[\"020a4da6-1111-443d-af80-3a425c558d14\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Indicators\":[\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"f9ae4976-7cd3-4718-834a-09349b6eb377\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"indicators\":[\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"f9ae4976-7cd3-4718-834a-09349b6eb377\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"),
                columns: new[] { "DataBlock_Request", "Name", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Indicators\":[\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"f9ae4976-7cd3-4718-834a-09349b6eb377\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, "[{\"indicators\":[\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"f9ae4976-7cd3-4718-834a-09349b6eb377\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d5f9b1f-8d0d-47d4-ba2b-ea97413d3117"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"fa0d7f1d-d181-43fb-955b-fc327da86f2c\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"e957db0c-3bf8-4e4b-5c6f-08d78f900a85\"],\"Indicators\":[\"d22e1104-de56-4617-4c2a-08d78f90080f\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"indicators\":[\"d22e1104-de56-4617-4c2a-08d78f90080f\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("695de169-947f-4f66-8564-6392b6113dfc"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"Indicators\":[\"68aeda43-2b6a-433a-4c06-08d78f90080f\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"indicators\":[\"68aeda43-2b6a-433a-4c06-08d78f90080f\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("9ccb0daf-91a1-4cb0-b3c1-2aed452338bc"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Indicators\":[\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"indicators\":[\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("a8c408ed-45d8-4690-a9f3-2fb0e86377bf"),
                columns: new[] { "DataBlock_Request", "Name", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"fa0d7f1d-d181-43fb-955b-fc327da86f2c\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2014\",\"StartCode\":\"CY\",\"EndYear\":\"2018\",\"EndCode\":\"CY\"},\"Filters\":[\"e957db0c-3bf8-4e4b-5c6f-08d78f900a85\"],\"Indicators\":[\"be1e1643-f7c8-40b0-4c2d-08d78f90080f\",\"16cdfc0a-f66f-496b-4c2e-08d78f90080f\",\"2c63589e-b5d4-4922-4c2f-08d78f90080f\",\"d10d4f10-c2f8-4120-4c30-08d78f90080f\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, "[{\"indicators\":[\"be1e1643-f7c8-40b0-4c2d-08d78f90080f\",\"16cdfc0a-f66f-496b-4c2e-08d78f90080f\",\"2c63589e-b5d4-4922-4c2f-08d78f90080f\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("d0397918-1697-40d8-b649-bea3c63c7d3e"),
                columns: new[] { "DataBlock_Request", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"Indicators\":[\"be3b765b-005f-4279-4c04-08d78f90080f\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", "[{\"indicators\":[\"be3b765b-005f-4279-4c04-08d78f90080f\"],\"tableHeaders\":null}]" });

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("dd572e49-87e3-46f5-bb04-e9008573fc91"),
                columns: new[] { "DataBlock_Request", "Name", "DataBlock_Tables" },
                values: new object[] { "{\"SubjectId\":\"3c0fbe56-0a4b-4caa-82f2-ab696cd96090\",\"GeographicLevel\":\"Country\",\"TimePeriod\":{\"StartYear\":\"2012\",\"StartCode\":\"AY\",\"EndYear\":\"2016\",\"EndCode\":\"AY\"},\"Filters\":[\"1f3f86a4-de9f-43d7-5bfd-08d78f900a85\"],\"Indicators\":[\"be3b765b-005f-4279-4c04-08d78f90080f\",\"a5a58f92-aba1-4955-4c02-08d78f90080f\",\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"Country\":null,\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null}", null, "[{\"indicators\":[\"a5a58f92-aba1-4955-4c02-08d78f90080f\",\"167f4807-4fdd-461a-4c03-08d78f90080f\",\"be3b765b-005f-4279-4c04-08d78f90080f\"],\"tableHeaders\":null}]" });
        }
    }
}
