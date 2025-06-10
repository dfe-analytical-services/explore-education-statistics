#nullable enable
using System.IO;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public abstract class AnalyticsPathResolverBase : IAnalyticsPathResolver
{
    protected abstract string GetBasePath();

    public string PublicZipDownloadsDirectoryPath()
    {
        return Path.Combine(GetBasePath(), "public", "zip-downloads");
    }

    public string PublicCsvDownloadsDirectoryPath()
    {
        return Path.Combine(GetBasePath(), "public", "csv-downloads");
    }
}
