#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
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
        public async Task<IList<ThemeTree<PublicationTreeNode>>> GetPublicationTree()
        {
            var themes = await _contentDbContext.Themes
                .Include(theme => theme.Topics)
                .ThenInclude(topic => topic.Publications)
                .ThenInclude(publication => publication.Releases)
                .ToListAsync();

            return themes
                .Where(theme => IsThemePublished(theme, IsPublicationPublishedOrHasLegacyUrl))
                .Select(
                    theme => new ThemeTree<PublicationTreeNode>()
                    {
                        Id = theme.Id,
                        Title = theme.Title,
                        Summary = theme.Summary,
                        Topics = theme.Topics
                            .Where(topic => IsTopicPublished(topic, IsPublicationPublishedOrHasLegacyUrl))
                            .Select(BuildTopicTree)
                            .OrderBy(topic => topic.Title)
                            .ToList()
                    }
                )
                .OrderBy(theme => theme.Title)
                .ToList();
        }

        private static bool IsPublicationPublishedOrHasLegacyUrl(Publication publication)
        {
            return !string.IsNullOrEmpty(publication.LegacyPublicationUrl?.ToString())
                   || publication.Releases.Any(release => release.IsLatestPublishedVersionOfRelease());
        }

        private static TopicTree<PublicationTreeNode> BuildTopicTree(Topic topic)
        {
            return new()
            {
                Id = topic.Id,
                Title = topic.Title,
                Publications = topic.Publications
                    .Where(IsPublicationPublishedOrHasLegacyUrl)
                    .Select(BuildPublicationNode)
                    .OrderBy(publication => publication.Title)
                    .ToList()
            };
        }

        private static PublicationTreeNode BuildPublicationNode(Publication publication)
        {
            // Ignore any legacyPublicationUrl once the Publication has Releases
            var legacyPublicationUrlIgnored =
                publication.Releases.Any(release => release.IsLatestPublishedVersionOfRelease());
            var legacyPublicationUrl =
                legacyPublicationUrlIgnored ? null : publication.LegacyPublicationUrl?.ToString();

            return new PublicationTreeNode
            {
                Id = publication.Id,
                Title = publication.Title,
                Summary = publication.Summary,
                Slug = publication.Slug,
                LegacyPublicationUrl = legacyPublicationUrl
            };
        }

        [BlobCache(typeof(PublicationDownloadsTreeCacheKey))]
        public async Task<IList<ThemeTree<PublicationDownloadsTreeNode>>> GetPublicationDownloadsTree()
        {
            var themes = await _contentDbContext.Themes
                .Include(theme => theme.Topics)
                .ThenInclude(topic => topic.Publications)
                .ThenInclude(publication => publication.Releases)
                .ToListAsync();

            var trees = await themes
                .Where(theme => IsThemePublished(theme))
                .SelectAsync(
                    async theme =>
                    {
                        var topics = await theme.Topics
                            .Where(topic => IsTopicPublished(topic))
                            .SelectAsync(async topic => await BuildTopicDownloadsTree(topic));

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
                );

            return trees
                .Where(theme => theme.Topics.Any())
                .OrderBy(theme => theme.Title)
                .ToList();
        }

        private async Task<TopicTree<PublicationDownloadsTreeNode>> BuildTopicDownloadsTree(Topic topic)
        {
            var publications = await topic.Publications
                .Where(publication => IsPublicationPublished(publication))
                .SelectAsync(async publication => await BuildPublicationDownloadsNode(publication));

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

        private static bool IsThemePublished(Theme theme, Func<Publication, bool>? publicationCriteria = null)
        {
            return theme.Topics.Any(topic => IsTopicPublished(topic, publicationCriteria));
        }

        private static bool IsTopicPublished(Topic topic, Func<Publication, bool>? publicationCriteria = null)
        {
            return topic.Publications.Any(publication => IsPublicationPublished(publication, publicationCriteria));
        }

        private static bool IsPublicationPublished(Publication publication, Func<Publication, bool>? criteria = null)
        {
            return criteria?.Invoke(publication) ??
                   publication.Releases.Any(release => release.IsLatestPublishedVersionOfRelease());
        }
    }
}