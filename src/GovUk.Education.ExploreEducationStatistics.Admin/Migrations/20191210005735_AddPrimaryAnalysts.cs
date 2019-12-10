using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations
{
    public partial class AddPrimaryAnalysts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7", "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "Primary Analyst", "PRIMARY ANALYST" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "6620bccf-2433-495e-995d-fc76c59d9c62", 0, "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "primary.analyst2@example.com", false, "Primary2", "Analyst2", true, null, "PRIMARY.ANALYST2@EXAMPLE.COM", "PRIMARY.ANALYST2@EXAMPLE.COM", null, null, false, "V7ZOUEOGN2HGZDN3HKPNIHLSUWWUHTA6", false, "primary.analyst2@example.com" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "e7f7c82e-aaf3-43db-a5ab-755678f67d04", 0, "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "primary.analyst1@example.com", false, "Primary1", "Analyst1", true, null, "PRIMARY.ANALYST1@EXAMPLE.COM", "PRIMARY.ANALYST1@EXAMPLE.COM", null, null, false, "V7ZOUEOGN2HGZDN3HKPNIHLSUWWUHTA6", false, "primary.analyst1@example.com" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { 1, "AdminAccessGranted", "", "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7" });

            migrationBuilder.InsertData(
                table: "AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey", "ProviderDisplayName", "UserId" },
                values: new object[,]
                {
                    { "OpenIdConnect", "5zzTEeAYz71aVPJ1ho1VGW3cYk7_qcQpkDqYYxbH3po", "OpenIdConnect", "e7f7c82e-aaf3-43db-a5ab-755678f67d04" },
                    { "OpenIdConnect", "RLdgJMsfN6QVjpCbkaOYIpzh6DA3QpRfnBcfIx46uDM", "OpenIdConnect", "6620bccf-2433-495e-995d-fc76c59d9c62" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[,]
                {
                    { "e7f7c82e-aaf3-43db-a5ab-755678f67d04", "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7" },
                    { "6620bccf-2433-495e-995d-fc76c59d9c62", "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetUserLogins",
                keyColumns: new[] { "LoginProvider", "ProviderKey" },
                keyValues: new object[] { "OpenIdConnect", "5zzTEeAYz71aVPJ1ho1VGW3cYk7_qcQpkDqYYxbH3po" });

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
                keyValues: new object[] { "e7f7c82e-aaf3-43db-a5ab-755678f67d04", "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "6620bccf-2433-495e-995d-fc76c59d9c62");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "e7f7c82e-aaf3-43db-a5ab-755678f67d04");
        }
    }
}
