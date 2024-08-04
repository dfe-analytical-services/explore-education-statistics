using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class EES4921_AddPublicIdToLocationOptionMetaChangeState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentState_PublicId",
                table: "LocationOptionMetaChanges",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousState_PublicId",
                table: "LocationOptionMetaChanges",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentState_PublicId",
                table: "LocationOptionMetaChanges");

            migrationBuilder.DropColumn(
                name: "PreviousState_PublicId",
                table: "LocationOptionMetaChanges");
        }
    }
}
