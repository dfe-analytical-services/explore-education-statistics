using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES3557_ReleaseRelatedInformationNotNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update all Releases with null RelatedInformation to have an empty array instead to match expectations in code
            // accessing the RelatedInformation property which expect a non-null List<Link> value.
            migrationBuilder.Sql("UPDATE dbo.Releases SET RelatedInformation = '[]' WHERE RelatedInformation IS NULL");

            // Make the RelatedInformation column not nullable now that every Release should have a value
            migrationBuilder.AlterColumn<string>(
                name: "RelatedInformation",
                table: "Releases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RelatedInformation",
                table: "Releases",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
