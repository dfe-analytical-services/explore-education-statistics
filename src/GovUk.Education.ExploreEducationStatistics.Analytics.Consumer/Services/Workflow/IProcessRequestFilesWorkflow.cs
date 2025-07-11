namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;

public interface IProcessRequestFilesWorkflow
{
    Task Process(IWorkflowActor actor);
}