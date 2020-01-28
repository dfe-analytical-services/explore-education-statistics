using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class AddPrereleaseUsersAndReleaseRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "LastName" },
                values: new object[,]
                {
                    { new Guid("ee9a02c1-b3f9-402c-9e9b-4fb78d737050"), "prerelease2@example.com", "Prerelease2", "User2" },
                    { new Guid("d5c85378-df85-482c-a1ce-09654dae567d"), "prerelease1@example.com", "Prerelease1", "User1" }
                });

            migrationBuilder.InsertData(
                table: "UserReleaseRoles",
                columns: new[] { "Id", "ReleaseId", "Role", "UserId" },
                values: new object[] { new Guid("69860a07-91d0-49d6-973d-98830fbbedfb"), new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), "PrereleaseViewer", new Guid("d5c85378-df85-482c-a1ce-09654dae567d") });

            migrationBuilder.InsertData(
                table: "UserReleaseRoles",
                columns: new[] { "Id", "ReleaseId", "Role", "UserId" },
                values: new object[] { new Guid("00cf98d9-c16c-4004-9fde-fe212b059845"), new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), "PrereleaseViewer", new Guid("d5c85378-df85-482c-a1ce-09654dae567d") });

            migrationBuilder.InsertData(
                table: "UserReleaseRoles",
                columns: new[] { "Id", "ReleaseId", "Role", "UserId" },
                values: new object[] { new Guid("ef19a735-81b4-482c-b1e2-31616ca26f51"), new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), "PrereleaseViewer", new Guid("ee9a02c1-b3f9-402c-9e9b-4fb78d737050") });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserReleaseRoles",
                keyColumn: "Id",
                keyValue: new Guid("00cf98d9-c16c-4004-9fde-fe212b059845"));

            migrationBuilder.DeleteData(
                table: "UserReleaseRoles",
                keyColumn: "Id",
                keyValue: new Guid("69860a07-91d0-49d6-973d-98830fbbedfb"));

            migrationBuilder.DeleteData(
                table: "UserReleaseRoles",
                keyColumn: "Id",
                keyValue: new Guid("ef19a735-81b4-482c-b1e2-31616ca26f51"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("d5c85378-df85-482c-a1ce-09654dae567d"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("ee9a02c1-b3f9-402c-9e9b-4fb78d737050"));
        }
    }
}
