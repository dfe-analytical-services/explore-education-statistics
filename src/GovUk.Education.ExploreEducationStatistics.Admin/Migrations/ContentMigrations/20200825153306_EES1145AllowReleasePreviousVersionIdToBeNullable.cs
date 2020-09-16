using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1145AllowReleasePreviousVersionIdToBeNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "PreviousVersionId",
                table: "Releases",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "PreviousVersionId",
                value: null);

            migrationBuilder.Sql(@"
                UPDATE Releases SET Releases.PreviousVersionId = null 
                WHERE Releases.PreviousVersionId = Releases.Id
                   OR Releases.PreviousVersionId = '00000000-0000-0000-0000-000000000000'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Releases SET Releases.PreviousVersionId = Releases.Id 
                WHERE Releases.PreviousVersionId IS NULL");

            migrationBuilder.AlterColumn<Guid>(
                name: "PreviousVersionId",
                table: "Releases",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "PreviousVersionId",
                value: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"));
        }
    }
}
