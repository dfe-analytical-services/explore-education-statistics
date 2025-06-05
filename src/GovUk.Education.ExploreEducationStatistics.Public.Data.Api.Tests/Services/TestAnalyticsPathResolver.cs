using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;

public class TestAnalyticsPathResolver : AnalyticsPathResolverBase, IDisposable
{
    private readonly string _basePath = Path.Combine(
        Path.GetTempPath(),
        "ExploreEducationStatistics",
        "Analytics",
        Guid.NewGuid().ToString());

    protected override string GetBasePath()
    {
        return _basePath;
    }
    
    public void Dispose()
    {
        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, recursive: true);
        }
    }
}
