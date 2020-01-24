using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations
{
    public partial class AddAnalystAndPrereleaseRolesAndUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "UserId", "RoleId" },
                keyValues: new object[] { "6620bccf-2433-495e-995d-fc76c59d9c62", "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "UserId", "RoleId" },
                keyValues: new object[] { "b390b405-ef90-4b9d-8770-22948e53189a", "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "UserId", "RoleId" },
                keyValues: new object[] { "e7f7c82e-aaf3-43db-a5ab-755678f67d04", "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "f9ddb43e-aa9e-41ed-837d-3062e130c425", "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "Analyst", "ANALYST" },
                    { "17e634f4-7a2b-4a23-8636-b079877b4232", "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "Prerelease User", "PRERELEASE USER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "d5c85378-df85-482c-a1ce-09654dae567d", 0, "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "prerelease1@example.com", false, "Prerelease1", "User1", true, null, "PRERELEASE1@EXAMPLE.COM", "PRERELEASE1@EXAMPLE.COM", null, null, false, "V7ZOUEOGN2HGZDN3HKPNIHLSUWWUHTA6", false, "prerelease1@example.com" },
                    { "ee9a02c1-b3f9-402c-9e9b-4fb78d737050", 0, "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "prerelease2@example.com", false, "Prerelease2", "User2", true, null, "PRERELEASE2@EXAMPLE.COM", "PRERELEASE2@EXAMPLE.COM", null, null, false, "V7ZOUEOGN2HGZDN3HKPNIHLSUWWUHTA6", false, "prerelease2@example.com" }
                });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { -13, "ApplicationAccessGranted", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -14, "AnalystPagesAccessGranted", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -15, "ApplicationAccessGranted", "", "17e634f4-7a2b-4a23-8636-b079877b4232" },
                    { -16, "PrereleasePagesAccessGranted", "", "17e634f4-7a2b-4a23-8636-b079877b4232" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey", "ProviderDisplayName", "UserId" },
                values: new object[,]
                {
                    { "OpenIdConnect", "uLGzMPaxGz0nY6nbff7wkBP7ly2iLdephomGPFOP0k8", "OpenIdConnect", "d5c85378-df85-482c-a1ce-09654dae567d" },
                    { "OpenIdConnect", "s5vNxMDGwRCvg3MTtLEDomZqOKl7cvv2f8PW5NvJzbw", "OpenIdConnect", "ee9a02c1-b3f9-402c-9e9b-4fb78d737050" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[,]
                {
                    { "e7f7c82e-aaf3-43db-a5ab-755678f67d04", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { "6620bccf-2433-495e-995d-fc76c59d9c62", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { "b390b405-ef90-4b9d-8770-22948e53189a", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { "d5c85378-df85-482c-a1ce-09654dae567d", "17e634f4-7a2b-4a23-8636-b079877b4232" },
                    { "ee9a02c1-b3f9-402c-9e9b-4fb78d737050", "17e634f4-7a2b-4a23-8636-b079877b4232" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: -16);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: -15);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: -14);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: -13);

            migrationBuilder.DeleteData(
                table: "AspNetUserLogins",
                keyColumns: new[] { "LoginProvider", "ProviderKey" },
                keyValues: new object[] { "OpenIdConnect", "s5vNxMDGwRCvg3MTtLEDomZqOKl7cvv2f8PW5NvJzbw" });

            migrationBuilder.DeleteData(
                table: "AspNetUserLogins",
                keyColumns: new[] { "LoginProvider", "ProviderKey" },
                keyValues: new object[] { "OpenIdConnect", "uLGzMPaxGz0nY6nbff7wkBP7ly2iLdephomGPFOP0k8" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "UserId", "RoleId" },
                keyValues: new object[] { "6620bccf-2433-495e-995d-fc76c59d9c62", "f9ddb43e-aa9e-41ed-837d-3062e130c425" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "UserId", "RoleId" },
                keyValues: new object[] { "b390b405-ef90-4b9d-8770-22948e53189a", "f9ddb43e-aa9e-41ed-837d-3062e130c425" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "UserId", "RoleId" },
                keyValues: new object[] { "d5c85378-df85-482c-a1ce-09654dae567d", "17e634f4-7a2b-4a23-8636-b079877b4232" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "UserId", "RoleId" },
                keyValues: new object[] { "e7f7c82e-aaf3-43db-a5ab-755678f67d04", "f9ddb43e-aa9e-41ed-837d-3062e130c425" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "UserId", "RoleId" },
                keyValues: new object[] { "ee9a02c1-b3f9-402c-9e9b-4fb78d737050", "17e634f4-7a2b-4a23-8636-b079877b4232" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "17e634f4-7a2b-4a23-8636-b079877b4232");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f9ddb43e-aa9e-41ed-837d-3062e130c425");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "d5c85378-df85-482c-a1ce-09654dae567d");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ee9a02c1-b3f9-402c-9e9b-4fb78d737050");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7", "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "Application User", "APPLICATION USER" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { -1, "ApplicationAccessGranted", "", "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[,]
                {
                    { "e7f7c82e-aaf3-43db-a5ab-755678f67d04", "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7" },
                    { "6620bccf-2433-495e-995d-fc76c59d9c62", "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7" },
                    { "b390b405-ef90-4b9d-8770-22948e53189a", "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7" }
                });
        }
    }
}
