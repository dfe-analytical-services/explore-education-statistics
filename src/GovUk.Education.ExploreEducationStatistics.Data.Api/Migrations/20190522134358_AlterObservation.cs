using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class AlterObservation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "NCI_WI_Observation_SubjectId",
                table: "Observation");

            migrationBuilder.AlterColumn<string>(
                name: "TimeIdentifier",
                table: "Observation",
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "GeographicLevel",
                table: "Observation",
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_Observation_GeographicLevel",
                table: "Observation",
                column: "GeographicLevel");
            
            migrationBuilder.Sql(
                "CREATE NONCLUSTERED INDEX [NCI_WI_Observation_SubjectId] ON [dbo].[Observation] ([SubjectId])" +
                " INCLUDE ([GeographicLevel], [LocationId], [TimeIdentifier], [Year]) WITH (ONLINE = ON)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "NCI_WI_Observation_SubjectId",
                table: "Observation");
            
            migrationBuilder.DropIndex(
                name: "IX_Observation_GeographicLevel",
                table: "Observation");

            migrationBuilder.AlterColumn<string>(
                name: "TimeIdentifier",
                table: "Observation",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 6);

            migrationBuilder.AlterColumn<string>(
                name: "GeographicLevel",
                table: "Observation",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 6);
            
            migrationBuilder.Sql(
                "CREATE NONCLUSTERED INDEX [NCI_WI_Observation_SubjectId] ON [dbo].[Observation] ([SubjectId])" +
                " INCLUDE ([LocationId], [TimeIdentifier], [Year]) WITH (ONLINE = ON)");
        }
    }
}
