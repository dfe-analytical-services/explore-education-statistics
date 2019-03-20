using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class SchoolPupilNumbersKeyStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"),
                column: "KeyStatistics",
                value: "[{\"Title\":\"Pupils in the school system\",\"Description\":\" Dolorum hic nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.\"},{\"Title\":\"Pupils eligible for and claiming free school meals \",\"Description\":\" Dolorum hic nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.\"},{\"Title\":\"Proportion of infant pupils in large classes\",\"Description\":\" Dolorum hic nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.\"}]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"),
                column: "KeyStatistics",
                value: "[]");
        }
    }
}
