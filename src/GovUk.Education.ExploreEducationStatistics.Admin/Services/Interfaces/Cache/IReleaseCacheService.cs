#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;

public interface IReleaseCacheService
{
    Task RemoveRelease(string publicationSlug, string releaseSlug);
}
