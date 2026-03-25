using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;

public interface IPublicationsTreeService
{
    Task<PublicationsTreeThemeDto[]> GetPublicationsTree(CancellationToken cancellationToken = default);

    Task<PublicationsTreeThemeDto[]> GetPublicationsTreeCached(CancellationToken cancellationToken = default);

    Task<PublicationsTreeThemeDto[]> GetPublicationsTreeFiltered(
        PublicationsTreeFilter filter,
        CancellationToken cancellationToken = default
    );

    Task<PublicationsTreeThemeDto[]> UpdateCachedPublicationsTree(CancellationToken cancellationToken = default);
}
