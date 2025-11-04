namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;

public interface IAnalyticsPathResolver
{
    string GetBasePath();

    string BuildSourceDirectory(string[] subPaths);
    string BuildReportsDirectory(string[] subPaths);
}
