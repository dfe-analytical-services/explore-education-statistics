#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;

public interface IPublicationCacheService
{
    Task RemovePublication(string publicationSlug);
}
