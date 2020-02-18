using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations
{
    public partial class AddCreateAndUpdatePermissionsForMethodologies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { -22, "CreateAnyMethodology", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -23, "UpdateAllMethodologies", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -24, "AccessAllMethodologies", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -25, "CreateAnyMethodology", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -26, "UpdateAllMethodologies", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -27, "AccessAllMethodologies", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: -27);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: -26);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: -25);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: -24);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: -23);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: -22);
        }
    }
}
