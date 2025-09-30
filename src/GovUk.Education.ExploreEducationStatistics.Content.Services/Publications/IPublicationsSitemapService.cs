using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;

public interface IPublicationsSitemapService
{
    Task<PublicationSitemapPublicationDto[]> GetSitemapItems(CancellationToken cancellationToken = default);
}
