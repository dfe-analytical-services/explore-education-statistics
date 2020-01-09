using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class AddApproverReleaseRoleToUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "UserReleaseRoles",
                columns: new[] { "Id", "ReleaseId", "Role", "UserId" },
                values: new object[,]
                {
                    { new Guid("d1cbc96e-75c0-424f-bd63-c1920b763020"), new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), "Approver", new Guid("b390b405-ef90-4b9d-8770-22948e53189a") },
                    { new Guid("1851e50d-04ac-4e16-911b-3df3350c589b"), new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), "Approver", new Guid("6620bccf-2433-495e-995d-fc76c59d9c62") }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserReleaseRoles",
                keyColumn: "Id",
                keyValue: new Guid("1851e50d-04ac-4e16-911b-3df3350c589b"));

            migrationBuilder.DeleteData(
                table: "UserReleaseRoles",
                keyColumn: "Id",
                keyValue: new Guid("d1cbc96e-75c0-424f-bd63-c1920b763020"));
        }
    }
}
