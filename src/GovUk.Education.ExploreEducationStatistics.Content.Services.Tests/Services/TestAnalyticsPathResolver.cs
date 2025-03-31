using System;
using System.IO;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Services;

public class TestAnalyticsPathResolver : IAnalyticsPathResolver
{
    private readonly string _basePath = Path.Combine(
        Path.GetTempPath(),
        "ExploreEducationStatistics",
        "Analytics",
        Guid.NewGuid().ToString());

    public string BasePath()
    {
        return _basePath;
    }

    public string PublicZipDownloadsDirectoryPath()
    {
        return Path.Combine(_basePath, "PublicZipDownloads");
    }
}
