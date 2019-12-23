using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations
{
    public partial class AddInitialUsersAndRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7", "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "Application User", "APPLICATION USER" },
                    { "cf67b697-bddd-41bd-86e0-11b7e11d99b3", "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "BAU User", "BAU USER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "e7f7c82e-aaf3-43db-a5ab-755678f67d04", 0, "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "analyst1@example.com", false, "Analyst1", "User1", true, null, "ANALYST1@EXAMPLE.COM", "ANALYST1@EXAMPLE.COM", null, null, false, "V7ZOUEOGN2HGZDN3HKPNIHLSUWWUHTA6", false, "analyst1@example.com" },
                    { "6620bccf-2433-495e-995d-fc76c59d9c62", 0, "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "analyst2@example.com", false, "Analyst2", "User2", true, null, "ANALYST2@EXAMPLE.COM", "ANALYST2@EXAMPLE.COM", null, null, false, "V7ZOUEOGN2HGZDN3HKPNIHLSUWWUHTA6", false, "analyst2@example.com" },
                    { "b390b405-ef90-4b9d-8770-22948e53189a", 0, "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "analyst3@example.com", false, "Analyst3", "User3", true, null, "ANALYST3@EXAMPLE.COM", "ANALYST3@EXAMPLE.COM", null, null, false, "V7ZOUEOGN2HGZDN3HKPNIHLSUWWUHTA6", false, "analyst3@example.com" },
                    { "b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd", 0, "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "bau1@example.com", false, "Bau1", "User1", true, null, "BAU1@EXAMPLE.COM", "BAU1@EXAMPLE.COM", null, null, false, "V7ZOUEOGN2HGZDN3HKPNIHLSUWWUHTA6", false, "bau1@example.com" },
                    { "b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63", 0, "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "bau2@example.com", false, "Bau2", "User2", true, null, "BAU2@EXAMPLE.COM", "BAU2@EXAMPLE.COM", null, null, false, "V7ZOUEOGN2HGZDN3HKPNIHLSUWWUHTA6", false, "bau2@example.com" }
                });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { -1, "ApplicationAccessGranted", "", "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7" },
                    { -2, "ApplicationAccessGranted", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -3, "AccessAllReleases", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -4, "AccessAllTopics", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey", "ProviderDisplayName", "UserId" },
                values: new object[,]
                {
                    { "OpenIdConnect", "5zzTEeAYz71aVPJ1ho1VGW3cYk7_qcQpkDqYYxbH3po", "OpenIdConnect", "e7f7c82e-aaf3-43db-a5ab-755678f67d04" },
                    { "OpenIdConnect", "RLdgJMsfN6QVjpCbkaOYIpzh6DA3QpRfnBcfIx46uDM", "OpenIdConnect", "6620bccf-2433-495e-995d-fc76c59d9c62" },
                    { "OpenIdConnect", "ces_f2I3zCjGZ9HUprWF3RiQgswrKvPFAY1Lwu_KI6M", "OpenIdConnect", "b390b405-ef90-4b9d-8770-22948e53189a" },
                    { "OpenIdConnect", "cb3XrjF6BLuMZ5P3aRo8wBobF7tAshdk2gF0X5Qm68o", "OpenIdConnect", "b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd" },
                    { "OpenIdConnect", "EKTK7hPGgxGVxRSBjgTv51XVJhtMo91sIcADfjSuJjw", "OpenIdConnect", "b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[,]
                {
                    { "e7f7c82e-aaf3-43db-a5ab-755678f67d04", "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7" },
                    { "6620bccf-2433-495e-995d-fc76c59d9c62", "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7" },
                    { "b390b405-ef90-4b9d-8770-22948e53189a", "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7" },
                    { "b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { "b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: -4);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: -3);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.DeleteData(
                table: "AspNetUserLogins",
                keyColumns: new[] { "LoginProvider", "ProviderKey" },
                keyValues: new object[] { "OpenIdConnect", "5zzTEeAYz71aVPJ1ho1VGW3cYk7_qcQpkDqYYxbH3po" });

            migrationBuilder.DeleteData(
                table: "AspNetUserLogins",
                keyColumns: new[] { "LoginProvider", "ProviderKey" },
                keyValues: new object[] { "OpenIdConnect", "cb3XrjF6BLuMZ5P3aRo8wBobF7tAshdk2gF0X5Qm68o" });

            migrationBuilder.DeleteData(
                table: "AspNetUserLogins",
                keyColumns: new[] { "LoginProvider", "ProviderKey" },
                keyValues: new object[] { "OpenIdConnect", "ces_f2I3zCjGZ9HUprWF3RiQgswrKvPFAY1Lwu_KI6M" });

            migrationBuilder.DeleteData(
                table: "AspNetUserLogins",
                keyColumns: new[] { "LoginProvider", "ProviderKey" },
                keyValues: new object[] { "OpenIdConnect", "EKTK7hPGgxGVxRSBjgTv51XVJhtMo91sIcADfjSuJjw" });

            migrationBuilder.DeleteData(
                table: "AspNetUserLogins",
                keyColumns: new[] { "LoginProvider", "ProviderKey" },
                keyValues: new object[] { "OpenIdConnect", "RLdgJMsfN6QVjpCbkaOYIpzh6DA3QpRfnBcfIx46uDM" });

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
                keyValues: new object[] { "b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "UserId", "RoleId" },
                keyValues: new object[] { "b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "UserId", "RoleId" },
                keyValues: new object[] { "e7f7c82e-aaf3-43db-a5ab-755678f67d04", "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cf67b697-bddd-41bd-86e0-11b7e11d99b3");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "6620bccf-2433-495e-995d-fc76c59d9c62");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b390b405-ef90-4b9d-8770-22948e53189a");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "e7f7c82e-aaf3-43db-a5ab-755678f67d04");
        }
    }
}
