#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics;

public interface IAnalyticsPathResolver
{
    string GetPermaLinkTableDownloadCallsDirectoryPath();

    string GetTableToolDownloadCallsDirectoryPath();
}
