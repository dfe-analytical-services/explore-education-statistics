#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public ThemeService(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

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

            return new TopicTree<PublicationTreeNode>
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
                case PublicationTreeFilter.AnyData:
                    return await publication.Releases
                        .ToAsyncEnumerable()
                        .AnyAwaitAsync(async release => release.IsLatestPublishedVersionOfRelease()
                                                        && await HasAnyDataFiles(release));

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
            var latestRelease = publication.LatestPublishedRelease();
            var type = latestRelease == null ? PublicationType.Legacy : GetPublicationType(latestRelease.Type);

            return new PublicationTreeNode
            {
                Id = publication.Id,
                Title = publication.Title,
                Slug = publication.Slug,
                Type = type,
                LegacyPublicationUrl = type == PublicationType.Legacy
                    ? publication.LegacyPublicationUrl?.ToString()
                    : null
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

        private static PublicationType GetPublicationType(ReleaseType releaseType)
        {
            return releaseType switch
            {
                ReleaseType.AdHocStatistics => PublicationType.AdHoc,
                ReleaseType.NationalStatistics => PublicationType.NationalAndOfficial,
                ReleaseType.ExperimentalStatistics => PublicationType.Experimental,
                ReleaseType.ManagementInformation => PublicationType.ManagementInformation,
                ReleaseType.OfficialStatistics => PublicationType.NationalAndOfficial,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
