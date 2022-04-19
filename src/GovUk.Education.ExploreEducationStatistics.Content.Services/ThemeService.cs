#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class ThemeService : IThemeService
    {
        private readonly ContentDbContext _contentDbContext;

        public ThemeService(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
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

        [BlobCache(typeof(PublicationTreeCacheKey))]
        private async Task<IList<ThemeTree<PublicationTreeNode>>> GetFullPublicationTree()
        {
            var themes = await _contentDbContext.Themes
                .Include(theme => theme.Topics)
                .ThenInclude(topic => topic.Publications)
                .ThenInclude(publication => publication.Releases)
                .ToListAsync();

            return await themes
                .ToAsyncEnumerable()
                .SelectAwait(async theme => await BuildThemeTree(theme))
                .Where(theme => theme.Topics.Any())
                .OrderBy(theme => theme.Title)
                .ToListAsync();
        }

        private async Task<ThemeTree<PublicationTreeNode>> BuildThemeTree(Theme theme)
        {
            var topics = await theme.Topics
                .ToAsyncEnumerable()
                .SelectAwait(async topic => await BuildTopicTree(topic))
                .Where(topic => topic.Publications.Any())
                .OrderBy(topic => topic.Title)
                .ToListAsync();

            return new ThemeTree<PublicationTreeNode>
            {
                Id = theme.Id,
                Title = theme.Title,
                Summary = theme.Summary,
                Topics = topics
            };
        }

        private async Task<TopicTree<PublicationTreeNode>> BuildTopicTree(Topic topic)
        {
            var publications = await topic.Publications
                .ToAsyncEnumerable()
                .Where(publication => publication
                                          .Releases
                                          .Any(r => r.IsLatestPublishedVersionOfRelease())
                                      || !string.IsNullOrEmpty(publication.LegacyPublicationUrl?.ToString()))
                .SelectAwait(async publication =>
                    await BuildPublicationNode(publication))
                .OrderBy(publication => publication.Title)
                .ToListAsync();

            return new TopicTree<PublicationTreeNode>
            {
                Id = topic.Id,
                Title = topic.Title,
                Publications = publications
            };
        }

        private async Task<PublicationTreeNode> BuildPublicationNode(Publication publication)
        {
            var latestRelease = publication.LatestPublishedRelease();
            var type = GetPublicationType(latestRelease?.Type);

            return new PublicationTreeNode
            {
                Id = publication.Id,
                Title = publication.Title,
                Slug = publication.Slug,
                Type = type,
                LegacyPublicationUrl = type == PublicationType.Legacy
                    ? publication.LegacyPublicationUrl?.ToString()
                    : null,
                IsSuperseded = IsSuperseded(publication),
                LatestReleaseHasData = latestRelease != null && await HasAnyDataFiles(latestRelease),
                AnyLiveReleaseHasData = await publication.Releases
                    .ToAsyncEnumerable()
                    .AnyAwaitAsync(async r => r.IsLatestPublishedVersionOfRelease()
                                              && await HasAnyDataFiles(r))
            };
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
                           && (publicationTreeNode.LatestReleaseHasData ||
                               !string.IsNullOrEmpty(publicationTreeNode.LegacyPublicationUrl));
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

        private bool IsSuperseded(Publication publication)
        {
            return publication.SupersededById != null
                   && _contentDbContext.Releases
                       .Include(r => r.Publication)
                       .Any(r => r.PublicationId == publication.SupersededById
                                 && r.Published.HasValue && DateTime.UtcNow >= r.Published.Value);
        }

        private async Task<bool> HasAnyDataFiles(Release release)
        {
            return await _contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .AnyAsync(rf => rf.ReleaseId == release.Id && rf.File.Type == FileType.Data);
        }

        private static PublicationType GetPublicationType(ReleaseType? releaseType)
        {
            return releaseType switch
            {
                ReleaseType.AdHocStatistics => PublicationType.AdHoc,
                ReleaseType.NationalStatistics => PublicationType.NationalAndOfficial,
                ReleaseType.ExperimentalStatistics => PublicationType.Experimental,
                ReleaseType.ManagementInformation => PublicationType.ManagementInformation,
                ReleaseType.OfficialStatistics => PublicationType.NationalAndOfficial,
                null => PublicationType.Legacy,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
