using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class AddUserAndReleaseRolesData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "LastName" },
                values: new object[,]
                {
                    { new Guid("b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63"), "bau2@example.com", "Bau2", "User2" },
                    { new Guid("b390b405-ef90-4b9d-8770-22948e53189a"), "analyst3@example.com", "Analyst3", "User3" },
                    { new Guid("6620bccf-2433-495e-995d-fc76c59d9c62"), "analyst2@example.com", "Analyst2", "User2" },
                    { new Guid("b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd"), "bau1@example.com", "Bau1", "User1" },
                    { new Guid("e7f7c82e-aaf3-43db-a5ab-755678f67d04"), "analyst1@example.com", "Analyst1", "User1" }
                });

            migrationBuilder.InsertData(
                table: "UserReleaseRoles",
                columns: new[] { "Id", "ReleaseId", "Role", "UserId" },
                values: new object[,]
                {
                    { new Guid("1501265c-979b-4cd4-8a55-00bfe909a2da"), new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), "Contributor", new Guid("e7f7c82e-aaf3-43db-a5ab-755678f67d04") },
                    { new Guid("239d8eed-8a7d-4f7a-ac0a-c20bc4e9167d"), new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), "Contributor", new Guid("e7f7c82e-aaf3-43db-a5ab-755678f67d04") },
                    { new Guid("086b1354-473c-48bb-9d30-0ac1963dc4cb"), new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), "Lead", new Guid("6620bccf-2433-495e-995d-fc76c59d9c62") },
                    { new Guid("e0dddf7a-f616-4e6f-bb9c-0b6e8ea3d9b9"), new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), "Contributor", new Guid("6620bccf-2433-495e-995d-fc76c59d9c62") },
                    { new Guid("77ff439d-e1cd-4e50-9c25-24a5207953a5"), new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), "Contributor", new Guid("6620bccf-2433-495e-995d-fc76c59d9c62") },
                    { new Guid("f7884899-baf9-4009-8561-f0c5df0d0a69"), new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), "Lead", new Guid("b390b405-ef90-4b9d-8770-22948e53189a") },
                    { new Guid("b00fd7c0-226f-474d-8cec-820a1a789182"), new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"), "Lead", new Guid("b390b405-ef90-4b9d-8770-22948e53189a") }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserReleaseRoles",
                keyColumn: "Id",
                keyValue: new Guid("086b1354-473c-48bb-9d30-0ac1963dc4cb"));

            migrationBuilder.DeleteData(
                table: "UserReleaseRoles",
                keyColumn: "Id",
                keyValue: new Guid("1501265c-979b-4cd4-8a55-00bfe909a2da"));

            migrationBuilder.DeleteData(
                table: "UserReleaseRoles",
                keyColumn: "Id",
                keyValue: new Guid("239d8eed-8a7d-4f7a-ac0a-c20bc4e9167d"));

            migrationBuilder.DeleteData(
                table: "UserReleaseRoles",
                keyColumn: "Id",
                keyValue: new Guid("77ff439d-e1cd-4e50-9c25-24a5207953a5"));

            migrationBuilder.DeleteData(
                table: "UserReleaseRoles",
                keyColumn: "Id",
                keyValue: new Guid("b00fd7c0-226f-474d-8cec-820a1a789182"));

            migrationBuilder.DeleteData(
                table: "UserReleaseRoles",
                keyColumn: "Id",
                keyValue: new Guid("e0dddf7a-f616-4e6f-bb9c-0b6e8ea3d9b9"));

            migrationBuilder.DeleteData(
                table: "UserReleaseRoles",
                keyColumn: "Id",
                keyValue: new Guid("f7884899-baf9-4009-8561-f0c5df0d0a69"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("6620bccf-2433-495e-995d-fc76c59d9c62"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b390b405-ef90-4b9d-8770-22948e53189a"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("e7f7c82e-aaf3-43db-a5ab-755678f67d04"));
        }
    }
}
