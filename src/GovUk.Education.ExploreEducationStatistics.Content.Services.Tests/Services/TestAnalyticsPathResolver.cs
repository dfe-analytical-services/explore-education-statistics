using System;
using System.IO;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Services;

public class TestAnalyticsPathResolver : AnalyticsPathResolverBase, IDisposable
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

    protected override string GetBasePath()
    {
        return _basePath;
    }
}
