using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations
{
    public partial class ESS1854UpdateDefaultUsersEmails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "6620bccf-2433-495e-995d-fc76c59d9c62",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "ees-analyst2@education.gov.uk", "EES-ANALYST2@EDUCATION.GOV.UK", "EES-ANALYST2@EDUCATION.GOV.UK", "ees-analyst2@education.gov.uk" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b390b405-ef90-4b9d-8770-22948e53189a",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "ees-analyst3@education.gov.uk", "EES-ANALYST3@EDUCATION.GOV.UK", "EES-ANALYST3@EDUCATION.GOV.UK", "ees-analyst3@education.gov.uk" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "ees-bau2@education.gov.uk", "EES-BAU2@EDUCATION.GOV.UK", "EES-BAU2@EDUCATION.GOV.UK", "ees-bau2@education.gov.uk" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "ees-bau1@education.gov.uk", "EES-BAU1@EDUCATION.GOV.UK", "EES-BAU1@EDUCATION.GOV.UK", "ees-bau1@education.gov.uk" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "d5c85378-df85-482c-a1ce-09654dae567d",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "ees-prerelease1@education.gov.uk", "EES-PRERELEASE1@EDUCATION.GOV.UK", "EES-PRERELEASE1@EDUCATION.GOV.UK", "ees-prerelease1@education.gov.uk" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "e7f7c82e-aaf3-43db-a5ab-755678f67d04",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "ees-analyst1@education.gov.uk", "EES-ANALYST1@EDUCATION.GOV.UK", "EES-ANALYST1@EDUCATION.GOV.UK", "ees-analyst1@education.gov.uk" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ee9a02c1-b3f9-402c-9e9b-4fb78d737050",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "ees-prerelease2@education.gov.uk", "EES-PRERELEASE2@EDUCATION.GOV.UK", "EES-PRERELEASE2@EDUCATION.GOV.UK", "ees-prerelease2@education.gov.uk" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "6620bccf-2433-495e-995d-fc76c59d9c62",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "analyst2@example.com", "ANALYST2@EXAMPLE.COM", "ANALYST2@EXAMPLE.COM", "analyst2@example.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b390b405-ef90-4b9d-8770-22948e53189a",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "analyst3@example.com", "ANALYST3@EXAMPLE.COM", "ANALYST3@EXAMPLE.COM", "analyst3@example.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "bau2@example.com", "BAU2@EXAMPLE.COM", "BAU2@EXAMPLE.COM", "bau2@example.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "bau1@example.com", "BAU1@EXAMPLE.COM", "BAU1@EXAMPLE.COM", "bau1@example.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "d5c85378-df85-482c-a1ce-09654dae567d",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "prerelease1@example.com", "PRERELEASE1@EXAMPLE.COM", "PRERELEASE1@EXAMPLE.COM", "prerelease1@example.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "e7f7c82e-aaf3-43db-a5ab-755678f67d04",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "analyst1@example.com", "ANALYST1@EXAMPLE.COM", "ANALYST1@EXAMPLE.COM", "analyst1@example.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ee9a02c1-b3f9-402c-9e9b-4fb78d737050",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "prerelease2@example.com", "PRERELEASE2@EXAMPLE.COM", "PRERELEASE2@EXAMPLE.COM", "prerelease2@example.com" });
        }
    }
}
