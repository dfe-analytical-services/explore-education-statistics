using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class UpdateReleaseSeedDataWithNewVersioningColumnValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                columns: new[] { "Created", "CreatedById", "OriginalId" },
                values: new object[] { new DateTime(2017, 8, 1, 23, 59, 54, 0, DateTimeKind.Utc), new Guid("b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd"), new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                columns: new[] { "Created", "CreatedById", "OriginalId" },
                values: new object[] { new DateTime(2019, 8, 1, 9, 30, 33, 0, DateTimeKind.Utc), new Guid("b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd"), new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717") });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                columns: new[] { "Created", "CreatedById", "OriginalId" },
                values: new object[] { new DateTime(2017, 8, 1, 11, 13, 22, 0, DateTimeKind.Utc), new Guid("b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd"), new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                columns: new[] { "Created", "CreatedById", "OriginalId" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                columns: new[] { "Created", "CreatedById", "OriginalId" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                columns: new[] { "Created", "CreatedById", "OriginalId" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), new Guid("00000000-0000-0000-0000-000000000000") });
        }
    }
}
