using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class Ees6644AddPoliceForceAreaToLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PoliceForceArea_Code",
                table: "Location",
                type: "nvarchar(max)",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "PoliceForceArea_Name",
                table: "Location",
                type: "nvarchar(max)",
                nullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Location_PoliceForceArea_Code",
                table: "Location",
                column: "PoliceForceArea_Code"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Location_PoliceForceArea_Code", table: "Location");

            migrationBuilder.DropColumn(name: "PoliceForceArea_Code", table: "Location");

            migrationBuilder.DropColumn(name: "PoliceForceArea_Name", table: "Location");
        }
    }
}
