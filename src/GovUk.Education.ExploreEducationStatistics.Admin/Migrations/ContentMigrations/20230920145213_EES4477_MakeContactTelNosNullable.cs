using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES4477_MakeContactTelNosNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ContactTelNo",
                table: "Contacts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Contacts]
                  SET ContactTelNo = NULL
                  WHERE TRIM(ContactTelNo) IN ('03700002288', '0370 000 2288', '037 0000 2288');");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ContactTelNo",
                table: "Contacts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
