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

public class ThemeCacheService : IThemeCacheService
{
    private readonly IThemeService _themeService;
    private readonly ILogger<ThemeCacheService> _logger;

    public ThemeCacheService(
        IThemeService themeService, 
        ILogger<ThemeCacheService> logger)
    {
        _themeService = themeService;
        _logger = logger;
    }

    [BlobCache(typeof(PublicationTreeCacheKey), ServiceName = "public")]
    private Task<IList<ThemeTree>> GetFullPublicationTree()
    {
        return _themeService.GetPublicationTree();
    }

    [BlobCache(typeof(PublicationTreeCacheKey), forceUpdate: true, ServiceName = "public")]
    public Task<IList<ThemeTree>> UpdatePublicationTree()
    {
        _logger.LogInformation("Updating cached Publication Tree");
        return _themeService.GetPublicationTree();
    }

    public async Task<Either<ActionResult, IList<ThemeTree>>> GetPublicationTree(
        PublicationTreeFilter filter)
    {
        var fullPublicationTree = await GetFullPublicationTree();

        return await fullPublicationTree
            .ToAsyncEnumerable()
            .SelectAwait(async theme => await FilterThemeTree(theme, filter))
            .Where(theme => theme.Topics.Any())
            .OrderBy(theme => theme.Title)
            .ToListAsync();
    }

    private async Task<ThemeTree> FilterThemeTree(
        ThemeTree themeTree,
        PublicationTreeFilter filter)
    {
        var topics = await themeTree.Topics
            .ToAsyncEnumerable()
            .SelectAwait(async topic => await FilterTopicTree(topic, filter))
            .Where(topic => topic.Publications.Any())
            .OrderBy(topic => topic.Title)
            .ToListAsync();

        return new ThemeTree
        {
            Id = themeTree.Id,
            Title = themeTree.Title,
            Summary = themeTree.Summary,
            Topics = topics,
        };
    }

    private async Task<TopicTree> FilterTopicTree(
        TopicTree topicTree,
        PublicationTreeFilter filter)
    {
        var publications = await topicTree.Publications
            .ToAsyncEnumerable()
            .Where(publication => FilterPublicationTreeNode(publication, filter))
            .OrderBy(publication => publication.Title)
            .ToListAsync();

        return new TopicTree
        {
            Id = topicTree.Id,
            Title = topicTree.Title,
            Publications = publications
        };
    }

    private bool FilterPublicationTreeNode(
        PublicationTreeNode publicationTreeNode,
        PublicationTreeFilter filter)
    {
        switch (filter)
        {
            case PublicationTreeFilter.FindStatistics:
                return !publicationTreeNode.IsSuperseded
                       && (publicationTreeNode.HasLiveRelease
                           || publicationTreeNode.Type == PublicationType.Legacy);
            case PublicationTreeFilter.DataTables:
                return publicationTreeNode.LatestReleaseHasData
                       && !publicationTreeNode.IsSuperseded;
            case PublicationTreeFilter.DataCatalogue:
            case PublicationTreeFilter.FastTrack:
                return publicationTreeNode.AnyLiveReleaseHasData;
            default:
                throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
        }
    }
}
