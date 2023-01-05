#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;

public class PublicationCacheService : IPublicationCacheService
{
    private readonly IPublicationService _publicationService;
    private readonly ILogger<PublicationCacheService> _logger;

    public PublicationCacheService(IPublicationService publicationService,
        ILogger<PublicationCacheService> logger)
    {
        _publicationService = publicationService;
        _logger = logger;
    }

    [BlobCache(typeof(PublicationCacheKey), ServiceName = "public")]
    public Task<Either<ActionResult, PublicationCacheViewModel>> GetPublication(string publicationSlug)
    {
        return _publicationService.Get(publicationSlug);
    }

    public async Task<Either<ActionResult, IList<PublicationTreeThemeViewModel>>> GetPublicationTree(
        PublicationTreeFilter filter)
    {
        var fullPublicationTree = await GetFullPublicationTree();

        return await fullPublicationTree
            .ToAsyncEnumerable()
            .SelectAwait(async theme => await FilterPublicationTreeTheme(theme, filter))
            .Where(theme => theme.Topics.Any())
            .OrderBy(theme => theme.Title)
            .ToListAsync();
    }

    [BlobCache(typeof(PublicationCacheKey), forceUpdate: true, ServiceName = "public")]
    public Task<Either<ActionResult, PublicationCacheViewModel>> UpdatePublication(string publicationSlug)
    {
        return _publicationService.Get(publicationSlug);
    }

    [BlobCache(typeof(PublicationTreeCacheKey), forceUpdate: true, ServiceName = "public")]
    public Task<IList<PublicationTreeThemeViewModel>> UpdatePublicationTree()
    {
        _logger.LogInformation("Updating cached Publication Tree");
        return _publicationService.GetPublicationTree();
    }

    [BlobCache(typeof(PublicationTreeCacheKey), ServiceName = "public")]
    private Task<IList<PublicationTreeThemeViewModel>> GetFullPublicationTree()
    {
        return _publicationService.GetPublicationTree();
    }

    private static async Task<PublicationTreeThemeViewModel> FilterPublicationTreeTheme(
        PublicationTreeThemeViewModel theme,
        PublicationTreeFilter filter)
    {
        var topics = await theme.Topics
            .ToAsyncEnumerable()
            .SelectAwait(async topic => await FilterPublicationTreeTopic(topic, filter))
            .Where(topic => topic.Publications.Any())
            .OrderBy(topic => topic.Title)
            .ToListAsync();

        return new PublicationTreeThemeViewModel
        {
            Id = theme.Id,
            Title = theme.Title,
            Summary = theme.Summary,
            Topics = topics,
        };
    }

    private static async Task<PublicationTreeTopicViewModel> FilterPublicationTreeTopic(
        PublicationTreeTopicViewModel topic,
        PublicationTreeFilter filter)
    {
        var publications = await topic.Publications
            .ToAsyncEnumerable()
            .Where(publication => FilterPublicationTreePublication(publication, filter))
            .OrderBy(publication => publication.Title)
            .ToListAsync();

        return new PublicationTreeTopicViewModel
        {
            Id = topic.Id,
            Title = topic.Title,
            Publications = publications
        };
    }

    private static bool FilterPublicationTreePublication(
        PublicationTreePublicationViewModel publication,
        PublicationTreeFilter filter)
    {
        switch (filter)
        {
            case PublicationTreeFilter.DataTables:
                return publication.LatestReleaseHasData
                       && !publication.IsSuperseded;
            case PublicationTreeFilter.DataCatalogue:
            case PublicationTreeFilter.FastTrack:
                return publication.AnyLiveReleaseHasData;
            default:
                throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
        }
    }
}
