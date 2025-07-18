#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Utils;

public interface IAnalyticsPathResolver
{
    string GetPermaLinkTableDownloadCallsDirectoryPath();

    string GetTableToolDownloadCallsDirectoryPath();
}
