using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES846AddContactToNonProductionEnvironments : Migration
    {
        private const string MigrationId = "20200511165307";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(ContentMigrationsPath, $"{MigrationId}_EES846_UpsertContact.sql");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("42a888c4-9ee7-40fd-9128-f5de546780b3"),
                column: "ContactId",
                value: new Guid("3b263e1d-9df3-45a4-acba-086c820db140"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("42a888c4-9ee7-40fd-9128-f5de546780b3"),
                column: "ContactId",
                value: null);

            migrationBuilder.DeleteData(
                table: "Contacts",
                keyColumn: "Id",
                keyValue: new Guid("3b263e1d-9df3-45a4-acba-086c820db140"));
        }
    }
}