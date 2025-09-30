using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.UsersAndRolesMigrations;

// ReSharper disable once InconsistentNaming
public partial class EES4376_AddBauSubmitMethodologyToHigherReviewPermission : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "AspNetRoleClaims",
            columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
            values: new object[] { -42, "SubmitAllMethodologiesToHigherReview", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "AspNetRoleClaims",
            keyColumn: "Id",
            keyValue: -42);
    }
}
