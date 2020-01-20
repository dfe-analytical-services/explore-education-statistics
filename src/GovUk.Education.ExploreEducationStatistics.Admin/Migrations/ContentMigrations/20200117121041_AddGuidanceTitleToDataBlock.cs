using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class AddGuidanceTitleToDataBlock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("02a637e7-6cc7-44e5-8991-8982edfe49fc"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"94f9b11c-df82-4eef-4c29-08d78f90080f\"],\"dataSummary\":[\"Down from 558,411 in 2017\"],\"dataDefinition\":[\"Total number of first preferences offered to applicants by schools.\"],\"dataDefinitionTitle\":[\"What is number of first preferences offered?\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("045a9585-688f-46fa-b3a9-9bdc237e0381"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"],\"dataSummary\":[\"Up from 1.1% in 2015/16\"],\"dataDefinition\":[\"Number of unauthorised absences as a percentage of the overall school population. <a href=\\\"/glossary#unauthorised-absence\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is unauthorized absence rate?\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("0b4c43cd-fc12-4159-88b9-0c8646424555"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"dataSummary\":[\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Total number of permanent exclusions within a school year. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is number of permanent exclusions?\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17251e1c-e978-419c-98f5-963131c952f7"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"dataSummary\":[\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Total number of permanent exclusions within a school year. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is number of permanent exclusions?\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"be3b765b-005f-4279-4c04-08d78f90080f\",\"68aeda43-2b6a-433a-4c06-08d78f90080f\",\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"dataSummary\":[\"Up from 0.08% in 2015/16\",\"Up from 4.29% in 2015/16\",\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Number of permanent exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\",\"Number of fixed-period exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\",\"Total number of permanent exclusions within a school year. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is permanent exclusion rate?\",\"What is fixed period exclusion rate?\",\"What is number of permanent exclusions?\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("1869d10a-ca3f-450c-9685-780b11d916f5"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"dataSummary\":[\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Total number of permanent exclusions within a school year. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is number of permanent exclusions?\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("3da30a08-9eeb-4a99-9872-796c3ea518fa"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"f9ae4976-7cd3-4718-834a-09349b6eb377\"],\"dataSummary\":[\"Similar to previous years\"],\"dataDefinition\":[\"Number of authorised absences as a percentage of the overall school population. <a href=\\\"/glossary#authorised-absence\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is authorized absence rate?\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"020a4da6-1111-443d-af80-3a425c558d14\",\"94f9b11c-df82-4eef-4c29-08d78f90080f\",\"d22e1104-de56-4617-4c2a-08d78f90080f\"],\"dataSummary\":[\"Down from 620,330 in 2017\",\"Down from 558,411 in 2017\",\"Down from 34,792 in 2017\"],\"dataDefinition\":[\"Total number of applications received for places at primary and secondary schools.\",\"Total number of first preferences offered to applicants by schools.\",\"Total number of second preferences offered to applicants by schools.\"],\"dataDefinitionTitle\":[\"What is number of applications received?\",\"What is number of first preferences offered?\",\"What is number of second preferences offered?\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5947759d-c6f3-451b-b353-a4da063f020a"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"020a4da6-1111-443d-af80-3a425c558d14\"],\"dataSummary\":[\"Down from 620,330 in 2017\"],\"dataDefinition\":[\"Total number of applications received for places at primary and secondary schools.\"],\"dataDefinitionTitle\":[\"What is number of applications received?\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"f9ae4976-7cd3-4718-834a-09349b6eb377\",\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"],\"dataSummary\":[\"Up from 4.6% in 2015/16\",\"Similar to previous years\",\"Up from 1.1% in 2015/16\"],\"dataDefinition\":[\"Total number of all authorised and unauthorised absences from possible school sessions for all pupils. <a href=\\\"/glossary#overall-absence\\\">More >>></a>\",\"Number of authorised absences as a percentage of the overall school population. <a href=\\\"/glossary#authorised-absence\\\">More >>></a>\",\"Number of unauthorised absences as a percentage of the overall school population. <a href=\\\"/glossary#unauthorised-absence\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is overall absence?\",\"What is authorized absence?\",\"What is unauthorized absence?\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d5f9b1f-8d0d-47d4-ba2b-ea97413d3117"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"d22e1104-de56-4617-4c2a-08d78f90080f\"],\"dataSummary\":[\"Down from 34,792 in 2017\"],\"dataDefinition\":[\"Total number of second preferences offered to applicants by schools.\"],\"dataDefinitionTitle\":[\"What is number of second preferences offered?\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("695de169-947f-4f66-8564-6392b6113dfc"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"68aeda43-2b6a-433a-4c06-08d78f90080f\"],\"dataSummary\":[\"Up from 4.29% in 2015/16\"],\"dataDefinition\":[\"Number of fixed-period exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is fixed period exclusion rate?\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("9ccb0daf-91a1-4cb0-b3c1-2aed452338bc"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\"],\"dataSummary\":[\"Up from 4.6% in 2015/16\"],\"dataDefinition\":[\"Total number of all authorised and unauthorised absences from possible school sessions for all pupils. <a href=\\\"/glossary#overall-absence\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is overall absence?\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("d0397918-1697-40d8-b649-bea3c63c7d3e"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"be3b765b-005f-4279-4c04-08d78f90080f\"],\"dataSummary\":[\"Up from 0.08% in 2015/16\"],\"dataDefinition\":[\"Number of permanent exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"dataDefinitionTitle\":[\"What is permanent exclusion rate?\"],\"description\":null}");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("02a637e7-6cc7-44e5-8991-8982edfe49fc"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"94f9b11c-df82-4eef-4c29-08d78f90080f\"],\"dataSummary\":[\"Down from 558,411 in 2017\"],\"dataDefinition\":[\"Total number of first preferences offered to applicants by schools.\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("045a9585-688f-46fa-b3a9-9bdc237e0381"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"],\"dataSummary\":[\"Up from 1.1% in 2015/16\"],\"dataDefinition\":[\"Number of unauthorised absences as a percentage of the overall school population. <a href=\\\"/glossary#unauthorised-absence\\\">More >>></a>\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("0b4c43cd-fc12-4159-88b9-0c8646424555"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"dataSummary\":[\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Total number of permanent exclusions within a school year. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17251e1c-e978-419c-98f5-963131c952f7"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"dataSummary\":[\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Total number of permanent exclusions within a school year. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"be3b765b-005f-4279-4c04-08d78f90080f\",\"68aeda43-2b6a-433a-4c06-08d78f90080f\",\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"dataSummary\":[\"Up from 0.08% in 2015/16\",\"Up from 4.29% in 2015/16\",\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Number of permanent exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\",\"Number of fixed-period exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\",\"Total number of permanent exclusions within a school year. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("1869d10a-ca3f-450c-9685-780b11d916f5"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"167f4807-4fdd-461a-4c03-08d78f90080f\"],\"dataSummary\":[\"Up from 6,685 in 2015/16\"],\"dataDefinition\":[\"Total number of permanent exclusions within a school year. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("3da30a08-9eeb-4a99-9872-796c3ea518fa"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"f9ae4976-7cd3-4718-834a-09349b6eb377\"],\"dataSummary\":[\"Similar to previous years\"],\"dataDefinition\":[\"Number of authorised absences as a percentage of the overall school population. <a href=\\\"/glossary#authorised-absence\\\">More >>></a>\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"020a4da6-1111-443d-af80-3a425c558d14\",\"94f9b11c-df82-4eef-4c29-08d78f90080f\",\"d22e1104-de56-4617-4c2a-08d78f90080f\"],\"dataSummary\":[\"Down from 620,330 in 2017\",\"Down from 558,411 in 2017\",\"Down from 34,792 in 2017\"],\"dataDefinition\":[\"Total number of applications received for places at primary and secondary schools.\",\"Total number of first preferences offered to applicants by schools.\",\"Total number of second preferences offered to applicants by schools.\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5947759d-c6f3-451b-b353-a4da063f020a"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"020a4da6-1111-443d-af80-3a425c558d14\"],\"dataSummary\":[\"Down from 620,330 in 2017\"],\"dataDefinition\":[\"Total number of applications received for places at primary and secondary schools.\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"f9ae4976-7cd3-4718-834a-09349b6eb377\",\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"],\"dataSummary\":[\"Up from 4.6% in 2015/16\",\"Similar to previous years\",\"Up from 1.1% in 2015/16\"],\"dataDefinition\":[\"Total number of all authorised and unauthorised absences from possible school sessions for all pupils. <a href=\\\"/glossary#overall-absence\\\">More >>></a>\",\"Number of authorised absences as a percentage of the overall school population. <a href=\\\"/glossary#authorised-absence\\\">More >>></a>\",\"Number of unauthorised absences as a percentage of the overall school population. <a href=\\\"/glossary#unauthorised-absence\\\">More >>></a>\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d5f9b1f-8d0d-47d4-ba2b-ea97413d3117"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"d22e1104-de56-4617-4c2a-08d78f90080f\"],\"dataSummary\":[\"Down from 34,792 in 2017\"],\"dataDefinition\":[\"Total number of second preferences offered to applicants by schools.\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("695de169-947f-4f66-8564-6392b6113dfc"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"68aeda43-2b6a-433a-4c06-08d78f90080f\"],\"dataSummary\":[\"Up from 4.29% in 2015/16\"],\"dataDefinition\":[\"Number of fixed-period exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("9ccb0daf-91a1-4cb0-b3c1-2aed452338bc"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\"],\"dataSummary\":[\"Up from 4.6% in 2015/16\"],\"dataDefinition\":[\"Total number of all authorised and unauthorised absences from possible school sessions for all pupils. <a href=\\\"/glossary#overall-absence\\\">More >>></a>\"],\"description\":null}");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("d0397918-1697-40d8-b649-bea3c63c7d3e"),
                column: "DataBlock_Summary",
                value: "{\"dataKeys\":[\"be3b765b-005f-4279-4c04-08d78f90080f\"],\"dataSummary\":[\"Up from 0.08% in 2015/16\"],\"dataDefinition\":[\"Number of permanent exclusions as a percentage of the overall school population. <a href=\\\"/glossary#permanent-exclusion\\\">More >>></a>\"],\"description\":null}");
        }
    }
}
