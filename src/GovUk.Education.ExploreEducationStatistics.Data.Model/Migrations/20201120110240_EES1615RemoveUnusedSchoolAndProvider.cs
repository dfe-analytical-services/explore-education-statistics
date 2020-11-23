using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1615RemoveUnusedSchoolAndProvider : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Observation_Provider_ProviderUrn",
                table: "Observation");

            migrationBuilder.DropForeignKey(
                name: "FK_Observation_School_SchoolLaEstab",
                table: "Observation");

            migrationBuilder.DropTable(
                name: "Provider");

            migrationBuilder.DropTable(
                name: "School");

            migrationBuilder.DropIndex(
                name: "IX_Observation_ProviderUrn",
                table: "Observation");

            migrationBuilder.DropIndex(
                name: "IX_Observation_SchoolLaEstab",
                table: "Observation");

            migrationBuilder.DropColumn(
                name: "ProviderUrn",
                table: "Observation");

            migrationBuilder.DropColumn(
                name: "SchoolLaEstab",
                table: "Observation");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProviderUrn",
                table: "Observation",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolLaEstab",
                table: "Observation",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Provider",
                columns: table => new
                {
                    Urn = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ukprn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Upin = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provider", x => x.Urn);
                });

            migrationBuilder.CreateTable(
                name: "School",
                columns: table => new
                {
                    LaEstab = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AcademyOpenDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcademyType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estab = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Postcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Urn = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_School", x => x.LaEstab);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Observation_ProviderUrn",
                table: "Observation",
                column: "ProviderUrn");

            migrationBuilder.CreateIndex(
                name: "IX_Observation_SchoolLaEstab",
                table: "Observation",
                column: "SchoolLaEstab");

            migrationBuilder.AddForeignKey(
                name: "FK_Observation_Provider_ProviderUrn",
                table: "Observation",
                column: "ProviderUrn",
                principalTable: "Provider",
                principalColumn: "Urn",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Observation_School_SchoolLaEstab",
                table: "Observation",
                column: "SchoolLaEstab",
                principalTable: "School",
                principalColumn: "LaEstab",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
