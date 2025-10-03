using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests;

public class TestAnalyticsPathResolver : AnalyticsPathResolverBase, IDisposable
{
    private readonly string _basePath = Path.Combine(
        Path.GetTempPath(),
        "ExploreEducationStatistics",
        "Analytics",
        Guid.NewGuid().ToString()
    );

    public void Dispose()
    {
        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, recursive: true);
        }
    }

    public override string GetBasePath()
    {
        return _basePath;
    }
}
