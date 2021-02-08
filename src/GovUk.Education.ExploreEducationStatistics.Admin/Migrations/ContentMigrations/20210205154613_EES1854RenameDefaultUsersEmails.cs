using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1854RenameDefaultUsersEmails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("6620bccf-2433-495e-995d-fc76c59d9c62"),
                column: "Email",
                value: "ees-analyst2@education.gov.uk");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b390b405-ef90-4b9d-8770-22948e53189a"),
                column: "Email",
                value: "ees-analyst3@education.gov.uk");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63"),
                column: "Email",
                value: "ees-bau2@education.gov.uk");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd"),
                column: "Email",
                value: "ees-bau1@education.gov.uk");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("d5c85378-df85-482c-a1ce-09654dae567d"),
                column: "Email",
                value: "ees-prerelease1@education.gov.uk");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("e7f7c82e-aaf3-43db-a5ab-755678f67d04"),
                column: "Email",
                value: "ees-analyst1@education.gov.uk");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("ee9a02c1-b3f9-402c-9e9b-4fb78d737050"),
                column: "Email",
                value: "ees-prerelease2@education.gov.uk");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("6620bccf-2433-495e-995d-fc76c59d9c62"),
                column: "Email",
                value: "analyst2@example.com");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b390b405-ef90-4b9d-8770-22948e53189a"),
                column: "Email",
                value: "analyst3@example.com");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63"),
                column: "Email",
                value: "bau2@example.com");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd"),
                column: "Email",
                value: "bau1@example.com");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("d5c85378-df85-482c-a1ce-09654dae567d"),
                column: "Email",
                value: "prerelease1@example.com");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("e7f7c82e-aaf3-43db-a5ab-755678f67d04"),
                column: "Email",
                value: "analyst1@example.com");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("ee9a02c1-b3f9-402c-9e9b-4fb78d737050"),
                column: "Email",
                value: "prerelease2@example.com");
        }
    }
}
