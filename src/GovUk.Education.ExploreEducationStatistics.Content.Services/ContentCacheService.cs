#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class ContentCacheService : IContentCacheService
{
    private readonly IMethodologyService _methodologyService;
    private readonly IThemeService _themeService;
    private readonly ILogger<ContentCacheService> _logger;

    public ContentCacheService(
        IMethodologyService methodologyService, 
        IThemeService themeService, 
        ILogger<ContentCacheService> logger)
    {
        _methodologyService = methodologyService;
        _themeService = themeService;
        _logger = logger;
    }

    [BlobCache(typeof(AllMethodologiesCacheKey))]
    public Task<Either<ActionResult, List<AllMethodologiesThemeViewModel>>> GetMethodologyTree()
    {
        return _methodologyService.GenerateSummariesTree();
    }

    [BlobCache(typeof(AllMethodologiesCacheKey), updateOnly: true)]
    public Task<Either<ActionResult, List<AllMethodologiesThemeViewModel>>> UpdateMethodologyTree()
    {
        _logger.LogInformation("Updating cached Methodology Tree");
        return _methodologyService.GenerateSummariesTree();
    }
    
    [BlobCache(typeof(PublicationTreeCacheKey))]
    private Task<IList<ThemeTree<PublicationTreeNode>>> GetFullPublicationTree()
    {
        return _themeService.GenerateFullPublicationTree();
    }
    
    [BlobCache(typeof(PublicationTreeCacheKey), updateOnly: true)]
    public Task<IList<ThemeTree<PublicationTreeNode>>> UpdatePublicationTree()
    {
        _logger.LogInformation("Updating cached Publication Tree");
        return _themeService.GenerateFullPublicationTree();
    }

    public Task<Either<ActionResult, List<MethodologyVersionSummaryViewModel>>> GetMethodologiesByPublication(Guid publicationId)
    {
        return GetMethodologyTree()
            .OnSuccess(methodologiesByTheme => 
                methodologiesByTheme
                    .SelectMany(theme => theme.Topics)
                    .SelectMany(topic => topic.Publications)
                    .SingleOrDefault(publication => publication.Id == publicationId))
            .OnSuccess(matchingPublication => matchingPublication?.Methodologies ?? new List<MethodologyVersionSummaryViewModel>());
    }
    
    public async Task<Either<ActionResult, IList<ThemeTree<PublicationTreeNode>>>> GetPublicationTree(
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

    private async Task<ThemeTree<PublicationTreeNode>> FilterThemeTree(
        ThemeTree<PublicationTreeNode> themeTree,
        PublicationTreeFilter filter)
    {
        var topics = await themeTree.Topics
            .ToAsyncEnumerable()
            .SelectAwait(async topic => await FilterTopicTree(topic, filter))
            .Where(topic => topic.Publications.Any())
            .OrderBy(topic => topic.Title)
            .ToListAsync();

        return new ThemeTree<PublicationTreeNode>
        {
            Id = themeTree.Id,
            Title = themeTree.Title,
            Summary = themeTree.Summary,
            Topics = topics,
        };
    }

    private async Task<TopicTree<PublicationTreeNode>> FilterTopicTree(
        TopicTree<PublicationTreeNode> topicTree,
        PublicationTreeFilter filter)
    {
        var publications = await topicTree.Publications
            .ToAsyncEnumerable()
            .Where(publication => FilterPublicationTreeNode(publication, filter))
            .OrderBy(publication => publication.Title)
            .ToListAsync();

        return new TopicTree<PublicationTreeNode>
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
