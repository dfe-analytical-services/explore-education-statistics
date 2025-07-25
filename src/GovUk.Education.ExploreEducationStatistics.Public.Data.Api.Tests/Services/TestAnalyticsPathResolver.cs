using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;

public class TestAnalyticsPathResolver : IAnalyticsPathResolver, IDisposable
{
    private readonly string _basePath = Path.Combine(
        Path.GetTempPath(),
        "ExploreEducationStatistics",
        "Analytics",
        Guid.NewGuid().ToString());

    public void Dispose()
    {
        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, recursive: true);
        }
    }

    public string BuildOutputDirectory(string[] subPaths) => Path.Combine([_basePath, ..subPaths]);
}
