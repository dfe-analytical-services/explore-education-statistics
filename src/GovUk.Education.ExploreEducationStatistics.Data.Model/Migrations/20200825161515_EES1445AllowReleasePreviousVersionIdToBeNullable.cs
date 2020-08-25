using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1445AllowReleasePreviousVersionIdToBeNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "PreviousVersionId",
                table: "Release",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql(@"
                UPDATE Release SET Release.PreviousVersionId = null 
                WHERE Release.PreviousVersionId = Release.Id 
                   OR Release.PreviousVersionId = '00000000-0000-0000-0000-000000000000'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Release SET Release.PreviousVersionId = Release.Id 
                WHERE Release.PreviousVersionId IS NULL");

            migrationBuilder.AlterColumn<Guid>(
                name: "PreviousVersionId",
                table: "Release",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);
        }
    }
}
