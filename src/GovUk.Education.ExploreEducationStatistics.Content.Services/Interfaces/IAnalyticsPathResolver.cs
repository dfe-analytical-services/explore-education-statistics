#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IAnalyticsPathResolver
{
    string PublicZipDownloadsDirectoryPath();

    string PublicCsvDownloadsDirectoryPath();
}
