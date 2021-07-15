using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES2385_AddMethodologyPublishingStrategy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublishingStrategy",
                table: "Methodologies",
                nullable: false,
                defaultValue: "");

            // Set all existing Methodologies to have Immediately as their publishing strategy
            migrationBuilder.Sql($"Update Methodologies SET PublishingStrategy='{Immediately.ToString()}'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishingStrategy",
                table: "Methodologies");
        }
    }
}
