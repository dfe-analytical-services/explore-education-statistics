namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface ICdnService
{
    Task PurgeCachePaths(string[] paths);
    Task PurgeReleaseAndSubpages(string publicationSlug, string releaseSlug);
}
