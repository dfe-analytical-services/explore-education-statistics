using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class DownloadService : IDownloadService
    {
        private readonly ContentDbContext _context;
        private readonly IReleaseService _releaseService;

        public DownloadService(ContentDbContext context,
            IReleaseService releaseService)
        {
            _context = context;
            _releaseService = releaseService;
        }

        public async Task<IEnumerable<ThemeTree<PublicationDownloadTreeNode>>> GetTree(IEnumerable<Guid> includedReleaseIds)
        {
            var themes = await _context.Themes
                .Include(theme => theme.Topics)
                .ThenInclude(topic => topic.Publications)
                .ThenInclude(publication => publication.Releases)
                .ToListAsync();

            var trees = await themes
                .Where(theme => IsThemePublished(theme, includedReleaseIds))
                .SelectAsync(async theme => await BuildThemeTree(theme, includedReleaseIds));
            return trees.OrderBy(theme => theme.Title);
        }

        private async Task<ThemeTree<PublicationDownloadTreeNode>> BuildThemeTree(Theme theme,
            IEnumerable<Guid> includedReleaseIds)
        {
            var topics = await theme.Topics
                .Where(topic => IsTopicPublished(topic, includedReleaseIds))
                .SelectAsync(topic => BuildTopicTree(topic, includedReleaseIds));

            return new ThemeTree<PublicationDownloadTreeNode>
            {
                Id = theme.Id,
                Title = theme.Title,
                Summary = theme.Summary,
                Topics = topics.OrderBy(topic => topic.Title).ToList()
            };
        }

        private async Task<TopicTree<PublicationDownloadTreeNode>> BuildTopicTree(Topic topic, IEnumerable<Guid> includedReleaseIds)
        {
            var publications = await topic.Publications
                .Where(publication => IsPublicationPublished(publication, includedReleaseIds))
                .SelectAsync(async publication => await BuildPublicationNode(publication, includedReleaseIds));

            return new TopicTree<PublicationDownloadTreeNode>
            {
                Id = topic.Id,
                Title = topic.Title,
                Publications = publications
                    .Where(publicationTree => publicationTree.DownloadFiles.Any())
                    .OrderBy(publication => publication.Title)
                    .ToList()
            };
        }

        private async Task <PublicationDownloadTreeNode> BuildPublicationNode(Publication publication,
            IEnumerable<Guid> includedReleaseIds)
        {
            var releases = publication.Releases
                .Where(r => IsReleasePublished(r, includedReleaseIds))
                .OrderBy(r => r.Year)
                .ThenBy(r => r.TimePeriodCoverage)
                .ToList();

            return new PublicationDownloadTreeNode
            {
                Id = publication.Id,
                Title = publication.Title,
                Summary = publication.Summary,
                Slug = publication.Slug,
                DownloadFiles = await GetDownloadFiles(publication, includedReleaseIds),
                EarliestReleaseTime = releases.FirstOrDefault()?.Title,
                LatestReleaseTime = releases.LastOrDefault()?.Title
            };
        }

        private async Task<List<FileInfo>> GetDownloadFiles(Publication publication, IEnumerable<Guid> includedReleaseIds)
        {
            var latestRelease = await _releaseService.GetLatestRelease(publication.Id, includedReleaseIds);
            return await _releaseService.GetDownloadFiles(latestRelease);
        }

        private static bool IsThemePublished(Theme theme, IEnumerable<Guid> includedReleaseIds)
        {
            return theme.Topics.Any(topic => IsTopicPublished(topic, includedReleaseIds));
        }

        private static bool IsTopicPublished(Topic topic, IEnumerable<Guid> includedReleaseIds)
        {
            return topic.Publications.Any(publication => IsPublicationPublished(publication, includedReleaseIds));
        }

        private static bool IsPublicationPublished(Publication publication, IEnumerable<Guid> includedReleaseIds)
        {
            return publication.Releases.Any(release => IsReleasePublished(release, includedReleaseIds));
        }

        private static bool IsReleasePublished(Release release, IEnumerable<Guid> includedReleaseIds)
        {
            return release.Live || includedReleaseIds.Contains(release.Id);
        }
    }
}