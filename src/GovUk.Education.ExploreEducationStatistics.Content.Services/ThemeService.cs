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
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class ThemeService : IThemeService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IReleaseFileService _releaseFileFileService;

        public ThemeService(ContentDbContext contentDbContext, IReleaseFileService releaseFileService)
        {
            _contentDbContext = contentDbContext;
            _releaseFileFileService = releaseFileService;
        }

        [BlobCache(typeof(PublicationTreeCacheKey))]
        public async Task<IList<ThemeTree<PublicationTreeNode>>> GetPublicationTree(
            PublicationTreeFilter? filter = null)
        {
            var themes = await ListThemes();

            return await themes
                .ToAsyncEnumerable()
                .SelectAwait(async theme => await BuildThemeTree(theme, filter))
                .Where(theme => theme.Topics.Any())
                .OrderBy(theme => theme.Title)
                .ToListAsync();
        }

        private async Task<ThemeTree<PublicationTreeNode>> BuildThemeTree(
            Theme theme,
            PublicationTreeFilter? filter)
        {
            var topics = await theme.Topics
                .ToAsyncEnumerable()
                .SelectAwait(async topic => await BuildTopicTree(topic, filter))
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

        private async Task<TopicTree<PublicationTreeNode>> BuildTopicTree(
            Topic topic,
            PublicationTreeFilter? filter)
        {
            var publications = await topic.Publications
                .ToAsyncEnumerable()
                .WhereAwait(async publication => await FilterPublication(publication, filter))
                .Select(BuildPublicationNode)
                .OrderBy(publication => publication.Title)
                .ToListAsync();

            return new()
            {
                Id = topic.Id,
                Title = topic.Title,
                Publications = publications
            };
        }

        private async Task<bool> FilterPublication(
            Publication publication,
            PublicationTreeFilter? filter)
        {
            switch (filter)
            {
                case PublicationTreeFilter.LatestData:
                    var latestLiveRelease = publication.LatestPublishedRelease();

                    return latestLiveRelease != null && await HasAnyDataFiles(latestLiveRelease);
                case null:
                    return !string.IsNullOrEmpty(publication.LegacyPublicationUrl?.ToString()) ||
                           publication.Releases.Any(release => release.IsLatestPublishedVersionOfRelease());
                default:
                    throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
            }
        }

        private async Task<bool> HasAnyDataFiles(Release release)
        {
            return await _contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .AnyAsync(rf => rf.ReleaseId == release.Id && rf.File.Type == FileType.Data);
        }

        private static PublicationTreeNode BuildPublicationNode(Publication publication)
        {
            // Ignore any legacyPublicationUrl once the Publication has Releases
            var legacyPublicationUrlIgnored =
                publication.Releases.Any(release => release.IsLatestPublishedVersionOfRelease());

            return new PublicationTreeNode
            {
                Id = publication.Id,
                Title = publication.Title,
                Summary = publication.Summary,
                Slug = publication.Slug,
                LegacyPublicationUrl = legacyPublicationUrlIgnored
                    ? null
                    : publication.LegacyPublicationUrl?.ToString()
            };
        }

        // TODO: EES-2365 Remove once 'Download latest data' page no longer exists
        [BlobCache(typeof(PublicationDownloadsTreeCacheKey))]
        public async Task<IList<ThemeTree<PublicationDownloadsTreeNode>>> GetPublicationDownloadsTree()
        {
            var themes = await ListThemes();

            return await themes
                .ToAsyncEnumerable()
                .SelectAwait(async theme => await BuildThemeDownloadsTree(theme))
                .Where(theme => theme.Topics.Any())
                .OrderBy(theme => theme.Title)
                .ToListAsync();
        }

        private async Task<ThemeTree<PublicationDownloadsTreeNode>> BuildThemeDownloadsTree(Theme theme)
        {
            var topics = await theme.Topics
                .ToAsyncEnumerable()
                .SelectAwait(async topic => await BuildTopicDownloadsTree(topic))
                .Where(topic => topic.Publications.Any())
                .ToListAsync();

            return new ThemeTree<PublicationDownloadsTreeNode>
            {
                Id = theme.Id,
                Title = theme.Title,
                Summary = theme.Summary,
                Topics = topics
                    .Where(topic => topic.Publications.Any())
                    .OrderBy(topic => topic.Title).ToList()
            };
        }

        private async Task<TopicTree<PublicationDownloadsTreeNode>> BuildTopicDownloadsTree(Topic topic)
        {
            var publications = await topic.Publications
                .ToAsyncEnumerable()
                .Where(IsPublicationPublished)
                .SelectAwait(async publication => await BuildPublicationDownloadsNode(publication))
                .ToListAsync();

            return new TopicTree<PublicationDownloadsTreeNode>
            {
                Id = topic.Id,
                Title = topic.Title,
                Publications = publications
                    .Where(publicationTree => publicationTree.DownloadFiles.Any())
                    .OrderBy(publication => publication.Title)
                    .ToList()
            };
        }

        private async Task<PublicationDownloadsTreeNode> BuildPublicationDownloadsNode(Publication publication)
        {
            var releases = publication.Releases
                .Where(r => r.IsLatestPublishedVersionOfRelease())
                .OrderBy(r => r.Year)
                .ThenBy(r => r.TimePeriodCoverage)
                .ToList();

            var latestRelease = releases.Last();

            return new PublicationDownloadsTreeNode
            {
                Id = publication.Id,
                Title = publication.Title,
                Summary = publication.Summary,
                Slug = publication.Slug,
                DownloadFiles = await _releaseFileFileService.ListDownloadFiles(latestRelease),
                EarliestReleaseTime = releases.FirstOrDefault()?.Title,
                LatestReleaseTime = releases.LastOrDefault()?.Title,
                LatestReleaseId = latestRelease.Id
            };
        }

        private async Task<List<Theme>> ListThemes()
        {
            return await _contentDbContext.Themes
                .Include(theme => theme.Topics)
                .ThenInclude(topic => topic.Publications)
                .ThenInclude(publication => publication.Releases)
                .ToListAsync();
        }

        private static bool IsPublicationPublished(Publication publication)
        {
            return publication.Releases.Any(release => release.IsLatestPublishedVersionOfRelease());
        }
    }
}