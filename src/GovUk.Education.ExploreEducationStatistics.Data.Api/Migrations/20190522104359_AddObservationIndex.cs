using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class AddObservationIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "CREATE NONCLUSTERED INDEX [NCI_WI_Observation_SubjectId] ON [dbo].[Observation] ([SubjectId])" +
                " INCLUDE ([LocationId], [TimeIdentifier], [Year]) WITH (ONLINE = ON)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX [dbo].[Observation].[NCI_WI_Observation_SubjectId]");
        }
    }
}