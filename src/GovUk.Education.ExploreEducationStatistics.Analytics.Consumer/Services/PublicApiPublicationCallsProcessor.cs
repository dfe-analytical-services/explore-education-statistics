using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicApiPublicationCallsProcessor(
    IAnalyticsPathResolver pathResolver,
    IProcessRequestFilesWorkflow workflow
) : IRequestFileProcessor
{
    private static readonly string[] PublicApiPublicationSubPath = ["public-api", "publications"];

    public string SourceDirectory => pathResolver.BuildSourceDirectory(PublicApiPublicationSubPath);
    public string ReportsDirectory => pathResolver.BuildReportsDirectory(PublicApiPublicationSubPath);

    public Task Process()
    {
        return workflow.Process(
            new WorkflowActor(sourceDirectory: SourceDirectory, reportsDirectory: ReportsDirectory)
        );
    }

    private class WorkflowActor(string sourceDirectory, string reportsDirectory)
        : DirectJsonCopyWorkflowActorBase(
            sourceDirectory: sourceDirectory,
            reportsDirectory: reportsDirectory,
            reportFilenamePart: "publications"
        )
    {
        public override async Task InitialiseDuckDb(DuckDbConnection connection)
        {
            await connection.ExecuteNonQueryAsync(
                @"
                CREATE TABLE sourceTable (
                    publicationId UUID,
                    publicationTitle VARCHAR,
                    parameters JSON,
                    startTime DATETIME,
                    type VARCHAR
                );
            "
            );
        }
    }
}
