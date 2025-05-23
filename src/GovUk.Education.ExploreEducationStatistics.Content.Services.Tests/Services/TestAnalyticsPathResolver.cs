using System;
using System.IO;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Services;

public class TestAnalyticsPathResolver : IAnalyticsPathResolver, IDisposable
{
    public void Dispose()
    {
        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, recursive: true);
        }
    }

    private readonly string _basePath = Path.Combine(
        Path.GetTempPath(),
        "ExploreEducationStatistics",
        "Analytics",
        Guid.NewGuid().ToString());

    public string PublicZipDownloadsDirectoryPath()
    {
        return Path.Combine(_basePath, "PublicZipDownloads");
    }

    public string PublicDataSetFileDownloadsDirectoryPath()
    {
        return Path.Combine(_basePath, "PublicDataSetFileDownloads");
    }
}
