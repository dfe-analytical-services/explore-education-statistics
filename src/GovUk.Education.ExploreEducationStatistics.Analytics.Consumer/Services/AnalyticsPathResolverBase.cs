using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public abstract class AnalyticsPathResolverBase : IAnalyticsPathResolver
{
    public abstract string GetBasePath();

    public string BuildSourceDirectory(string[] subPaths) =>
        Path.Combine([GetBasePath(), .. subPaths]);

    public string BuildReportsDirectory(string[] subPaths) =>
        Path.Combine([ReportsDirectoryPath(), .. subPaths]);

    private string ReportsDirectoryPath() => Path.Combine(GetBasePath(), "reports");
}
