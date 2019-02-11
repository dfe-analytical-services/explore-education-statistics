using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class KeyStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KeyStatistics",
                table: "Releases",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "KeyStatistics",
                value: "[{\"Title\":\"Overall absence\",\"Description\":\"Overall absence is the adipisicing elit. Dolorum hic nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.\"},{\"Title\":\"Authorised absence\",\"Description\":\"Authorised absence is the adipisicing elit. Dolorum hic nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.\"},{\"Title\":\"Unauthorised absence\",\"Description\":\"Unauthorised absence is the adipisicing elit. Dolorum hic nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.\"}]");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"),
                column: "KeyStatistics",
                value: "[]");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "KeyStatistics",
                value: "[{\"Title\":\"Overall permanent exclusions\",\"Description\":\"Overall permanent exclusions is the adipisicing elit. Dolorum hic nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.\"},{\"Title\":\"Number of exclusions\",\"Description\":\"Number of exclusions is the adipisicing elit. Dolorum hic nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.\"},{\"Title\":\"Overall rate of fixed-period exclusions\",\"Description\":\"Overall rate of fixed-period exclusionsis the adipisicing elit. Dolorum hic nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.\"}]");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                column: "KeyStatistics",
                value: "[{\"Title\":\"Overall absence\",\"Description\":\"Overall absence is the adipisicing elit. Dolorum hic nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.\"},{\"Title\":\"Authorised absence\",\"Description\":\"Authorised absence is the adipisicing elit. Dolorum hic nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.\"},{\"Title\":\"Unauthorised absence\",\"Description\":\"Unauthorised absence is the adipisicing elit. Dolorum hic nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.\"}]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeyStatistics",
                table: "Releases");
        }
    }
}
