using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicApiTopLevelCallsProcessor(
    IAnalyticsPathResolver pathResolver,
    IProcessRequestFilesWorkflow workflow) : IRequestFileProcessor
{
    public Task Process()
    {
        return workflow.Process(new WorkflowActor(
            sourceDirectory: pathResolver.PublicApiTopLevelCallsDirectoryPath(),
            reportsDirectory: pathResolver.PublicApiTopLevelCallsReportsDirectoryPath()));
    }

    private class WorkflowActor(string sourceDirectory, string reportsDirectory) 
        : DirectJsonCopyWorkflowActorBase(
            sourceDirectory: sourceDirectory,
            reportsDirectory: reportsDirectory,
            reportFilenamePart: "top-level")
    {
        public override async Task InitialiseDuckDb(DuckDbConnection connection)
        {
            await connection.ExecuteNonQueryAsync(@"
                CREATE TABLE sourceTable (
                    parameters JSON,
                    startTime DATETIME,
                    type VARCHAR
                );
            ");
        }
    }
}
