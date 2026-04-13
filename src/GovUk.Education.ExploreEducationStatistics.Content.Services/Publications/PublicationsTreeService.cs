using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;

public class PublicationsTreeService(
    ContentDbContext contentDbContext,
    IPublicationRepository publicationRepository,
    IPublicBlobCacheService publicBlobCacheService,
    IReleaseVersionRepository releaseVersionRepository,
    ILogger<PublicationsTreeService> logger
) : IPublicationsTreeService
{
    internal async Task<PublicationsTreeThemeDto[]> GetPublicationsTree(CancellationToken cancellationToken = default)
    {
        var themes = await contentDbContext
            .Themes.Include(theme => theme.Publications)
                .ThenInclude(publication => publication.SupersededBy)
            .ToListAsync(cancellationToken);

        return await themes
            .ToAsyncEnumerable()
            .Select(async (theme, ct) => await BuildPublicationTreeTheme(theme, cancellationToken: ct))
            .Where(theme => theme.Publications.Length > 0)
            .OrderBy(theme => theme.Title)
            .ToArrayAsync(cancellationToken);
    }

    internal async Task<PublicationsTreeThemeDto[]> GetPublicationsTreeCached(
        CancellationToken cancellationToken = default
    ) =>
        await publicBlobCacheService.GetOrCreateAsync(
            cacheKey: new PublicationsTreeCacheKey(),
            createIfNotExistsFn: () => GetPublicationsTree(cancellationToken),
            logger: logger
        );

    public async Task<PublicationsTreeThemeDto[]> GetPublicationsTreeFiltered(
        PublicationsTreeFilter filter,
        CancellationToken cancellationToken = default
    )
    {
        var tree = await GetPublicationsTreeCached(cancellationToken);
        return ApplyFilter(tree, filter);
    }

    public Task<PublicationsTreeThemeDto[]> UpdateCachedPublicationsTree(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating cached Publication Tree");

        return publicBlobCacheService.Update(
            cacheKey: new PublicationsTreeCacheKey(),
            createFn: () => GetPublicationsTree(cancellationToken),
            logger: logger
        );
    }

    private async Task<PublicationsTreeThemeDto> BuildPublicationTreeTheme(
        Theme theme,
        CancellationToken cancellationToken
    )
    {
        var publications = await theme
            .Publications.Where(publication => publication.LatestPublishedReleaseVersionId != null)
            .ToAsyncEnumerable()
            .Select<Publication, PublicationsTreePublicationDto>(
                async (publication, ct) => await BuildPublicationTreePublication(publication, cancellationToken: ct)
            )
            .OrderBy(publication => publication.Title)
            .ToArrayAsync(cancellationToken);

        return PublicationsTreeThemeDto.FromTheme(theme, publications);
    }

    private async Task<PublicationsTreePublicationDto> BuildPublicationTreePublication(
        Publication publication,
        CancellationToken cancellationToken
    )
    {
        var isSuperseded = await publicationRepository.IsSuperseded(publication.Id);

        var latestPublishedReleaseVersionId = publication.LatestPublishedReleaseVersionId;
        var latestReleaseHasData =
            latestPublishedReleaseVersionId.HasValue
            && await ReleaseVersionHasAnyDataFiles(latestPublishedReleaseVersionId.Value, cancellationToken);

        var publishedReleaseVersionIds = await releaseVersionRepository.ListLatestReleaseVersionIds(
            publication.Id,
            publishedOnly: true,
            cancellationToken
        );

        var anyLiveReleaseHasData = await publishedReleaseVersionIds
            .ToAsyncEnumerable()
            .AnyAsync(
                async (releaseVersionId, ct) =>
                    await ReleaseVersionHasAnyDataFiles(releaseVersionId, cancellationToken: ct),
                cancellationToken
            );

        return PublicationsTreePublicationDto.FromPublication(
            publication,
            isSuperseded: isSuperseded,
            anyLiveReleaseHasData: anyLiveReleaseHasData,
            latestReleaseHasData: latestReleaseHasData
        );
    }

    private async Task<bool> ReleaseVersionHasAnyDataFiles(
        Guid releaseVersionId,
        CancellationToken cancellationToken
    ) =>
        await contentDbContext
            .ReleaseFiles.Include(rf => rf.File)
            .AnyAsync(
                rf => rf.ReleaseVersionId == releaseVersionId && rf.File.Type == FileType.Data,
                cancellationToken
            );

    private static PublicationsTreeThemeDto[] ApplyFilter(
        PublicationsTreeThemeDto[] tree,
        PublicationsTreeFilter filter
    ) =>
        tree.Select(theme =>
                theme with
                {
                    Publications =
                    [
                        .. theme.Publications.Where(publication => PublicationMatchesFilter(publication, filter)),
                    ],
                }
            )
            .Where(theme => theme.Publications.Length > 0)
            .ToArray();

    private static bool PublicationMatchesFilter(
        PublicationsTreePublicationDto publication,
        PublicationsTreeFilter filter
    ) =>
        filter switch
        {
            PublicationsTreeFilter.DataTables => publication.LatestReleaseHasData,
            PublicationsTreeFilter.DataCatalogue or PublicationsTreeFilter.FastTrack =>
                publication.AnyLiveReleaseHasData,
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null),
        };
}
