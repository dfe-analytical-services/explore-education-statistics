using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicApiTopLevelCallsProcessor(
    IAnalyticsPathResolver pathResolver,
    IProcessRequestFilesWorkflow workflow) : IRequestFileProcessor
{
    private static readonly string[] PublicApiTopLevelSubPath = ["public-api", "top-level"];

    public string SourceDirectory => pathResolver.BuildSourceDirectory(PublicApiTopLevelSubPath);
    public string ReportsDirectory => pathResolver.BuildReportsDirectory(PublicApiTopLevelSubPath);

    public Task Process()
    {
        return workflow.Process(new WorkflowActor(
            sourceDirectory: SourceDirectory,
            reportsDirectory: ReportsDirectory));
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
