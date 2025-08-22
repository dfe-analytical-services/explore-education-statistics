#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;

public interface IPublicationsSitemapService
{
    Task<PublicationSitemapItemDto[]> GetSitemapItems(CancellationToken cancellationToken = default);
}
