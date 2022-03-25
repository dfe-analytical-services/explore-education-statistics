using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES3067_DropObservationGeographicLevel : Migration
    {
        private const string PreviousObservationTypeMigrationId = InitialCreate_Custom.MigrationId;
        private const string PreviousInsertObservationsMigrationId = InitialCreate_Custom.MigrationId;
        private const string MigrationId = "20220325160924";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Recreate the non clustered index NCI_WI_Observation_SubjectId without the GeographicLevel column
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX NCI_WI_Observation_SubjectId ON dbo.Observation (SubjectId) INCLUDE (LocationId, TimeIdentifier, Year) WITH (ONLINE = ON, DROP_EXISTING = ON);");

            // Drop the index on the GeographicLevel column
            migrationBuilder.DropIndex(
                name: "IX_Observation_GeographicLevel",
                table: "Observation");

            // Drop the GeographicLevel column
            migrationBuilder.DropColumn(
                name: "GeographicLevel",
                table: "Observation");

            // Update ObservationType used by the Importer
            migrationBuilder.Sql("DROP PROCEDURE InsertObservations");
            migrationBuilder.Sql("DROP TYPE ObservationType");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_TableType_ObservationType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousInsertObservationsMigrationId}_Routine_InsertObservations.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeographicLevel",
                table: "Observation",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Observation_GeographicLevel",
                table: "Observation",
                column: "GeographicLevel");

            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX NCI_WI_Observation_SubjectId ON dbo.Observation (SubjectId) INCLUDE (GeographicLevel, LocationId, TimeIdentifier, Year) WITH (ONLINE = ON, DROP_EXISTING = ON);");

            migrationBuilder.Sql("DROP PROCEDURE InsertObservations");
            migrationBuilder.Sql("DROP TYPE ObservationType");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousObservationTypeMigrationId}_TableType_ObservationType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousInsertObservationsMigrationId}_Routine_InsertObservations.sql");
        }
    }
}
