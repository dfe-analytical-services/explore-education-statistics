using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.UsersAndRolesMigrations
{
    /// <inheritdoc />
    public partial class Ees7228MigratingAnalystAndPrereleaseRolesToStandardUser : Migration
    {
        private const string MigrationId = "20260717111019";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migrating Analyst and Prerelease users to the new Standard User role, and removing the Analyst and Prerelease roles from the system.
            migrationBuilder.SqlFromFile(
                MigrationConstants.UsersAndRolesMigrationsPath,
                $"{MigrationId}_{nameof(Ees7228MigratingAnalystAndPrereleaseRolesToStandardUser)}.sql"
            );

            migrationBuilder.DeleteData(table: "AspNetRoleClaims", keyColumn: "Id", keyValue: -21);

            migrationBuilder.DeleteData(table: "AspNetRoleClaims", keyColumn: "Id", keyValue: -20);

            migrationBuilder.DeleteData(table: "AspNetRoleClaims", keyColumn: "Id", keyValue: -19);

            migrationBuilder.DeleteData(table: "AspNetRoleClaims", keyColumn: "Id", keyValue: -18);

            migrationBuilder.DeleteData(table: "AspNetRoleClaims", keyColumn: "Id", keyValue: -17);

            migrationBuilder.DeleteData(table: "AspNetRoleClaims", keyColumn: "Id", keyValue: -16);

            migrationBuilder.DeleteData(table: "AspNetRoleClaims", keyColumn: "Id", keyValue: -15);

            migrationBuilder.DeleteData(table: "AspNetRoleClaims", keyColumn: "Id", keyValue: -14);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "17e634f4-7a2b-4a23-8636-b079877b4232"
            );

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f9ddb43e-aa9e-41ed-837d-3062e130c425",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "Standard User", "STANDARD USER" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
