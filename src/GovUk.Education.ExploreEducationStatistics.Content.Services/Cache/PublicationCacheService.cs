using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;

public class PublicationCacheService(
    IPublicationService publicationService,
    IPublicBlobStorageService publicBlobStorageService,
    IPublicBlobCacheService publicBlobCacheService,
    ILogger<PublicationCacheService> logger
) : IPublicationCacheService
{
    public Task<Either<ActionResult, PublicationCacheViewModel>> GetPublication(string publicationSlug)
    {
        return publicBlobCacheService.GetOrCreateAsync(
            new PublicationCacheKey(PublicationSlug: publicationSlug),
            () => publicationService.Get(publicationSlug),
            logger: logger
        );
    }

    public async Task<Either<ActionResult, IList<PublicationTreeThemeViewModel>>> GetPublicationTree(
        PublicationTreeFilter filter
    )
    {
        var fullPublicationTree = await GetFullPublicationTree();

        return await fullPublicationTree
            .ToAsyncEnumerable()
            .SelectAwait(async theme => await FilterPublicationTreeTheme(theme, filter))
            .Where(theme => theme.Publications.Any())
            .OrderBy(theme => theme.Title)
            .ToListAsync();
    }

    public Task<Either<ActionResult, PublicationCacheViewModel>> UpdatePublication(string publicationSlug)
    {
        return publicBlobCacheService.Update(
            new PublicationCacheKey(PublicationSlug: publicationSlug),
            () => publicationService.Get(publicationSlug),
            logger: logger
        );
    }

    public async Task<Either<ActionResult, Unit>> RemovePublication(string publicationSlug)
    {
        await publicBlobStorageService.DeleteBlobs(
            containerName: BlobContainers.PublicContent,
            options: new IBlobStorageService.DeleteBlobsOptions
            {
                IncludeRegex = new Regex($"^publications/{publicationSlug}/.+$"),
            }
        );

        return Unit.Instance;
    }

    public Task<IList<PublicationTreeThemeViewModel>> UpdatePublicationTree()
    {
        logger.LogInformation("Updating cached Publication Tree");

        return publicBlobCacheService.Update(
            cacheKey: new PublicationTreeCacheKey(),
            createFn: publicationService.GetPublicationTree,
            logger: logger
        );
    }

    private Task<IList<PublicationTreeThemeViewModel>> GetFullPublicationTree()
    {
        return publicBlobCacheService.GetOrCreateAsync(
            cacheKey: new PublicationTreeCacheKey(),
            createIfNotExistsFn: publicationService.GetPublicationTree,
            logger: logger
        );
    }

    private static async Task<PublicationTreeThemeViewModel> FilterPublicationTreeTheme(
        PublicationTreeThemeViewModel theme,
        PublicationTreeFilter filter
    )
    {
        var publications = await theme
            .Publications.ToAsyncEnumerable()
            .Where(publication => FilterPublicationTreePublication(publication, filter))
            .OrderBy(publication => publication.Title)
            .ToListAsync();

        return new PublicationTreeThemeViewModel
        {
            Id = theme.Id,
            Title = theme.Title,
            Summary = theme.Summary,
            Publications = publications,
        };
    }

    private static bool FilterPublicationTreePublication(
        PublicationTreePublicationViewModel publication,
        PublicationTreeFilter filter
    )
    {
        switch (filter)
        {
            case PublicationTreeFilter.DataTables:
                return publication.LatestReleaseHasData;
            case PublicationTreeFilter.DataCatalogue:
            case PublicationTreeFilter.FastTrack:
                return publication.AnyLiveReleaseHasData;
            default:
                throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
        }
    }
}
