#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class ThemeService : IThemeService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPublicationRepository _publicationRepository;

        public ThemeService(ContentDbContext contentDbContext,
            IPublicationRepository publicationRepository)
        {
            _contentDbContext = contentDbContext;
            _publicationRepository = publicationRepository;
        }

        public async Task<IList<ThemeTree<PublicationTreeNode>>> GetPublicationTree()
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
                .Where(publication =>
                    publication.LatestPublishedReleaseId != null || publication.LegacyPublicationUrl != null)
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
            var type = await GetPublicationType(publication);
            var latestPublishedReleaseId = publication.LatestPublishedReleaseId;

            return new PublicationTreeNode
            {
                Id = publication.Id,
                Title = publication.Title,
                Slug = publication.Slug,
                Type = type,
                LegacyPublicationUrl = type == PublicationType.Legacy
                    ? publication.LegacyPublicationUrl?.ToString()
                    : null,
                IsSuperseded = await _publicationRepository.IsSuperseded(publication.Id),
                HasLiveRelease = latestPublishedReleaseId != null,
                LatestReleaseHasData = latestPublishedReleaseId != null &&
                                       await HasAnyDataFiles(latestPublishedReleaseId.Value),
                AnyLiveReleaseHasData = await publication.Releases
                    .ToAsyncEnumerable()
                    .AnyAwaitAsync(async r => r.IsLatestPublishedVersionOfRelease()
                                              && await HasAnyDataFiles(r.Id))
            };
        }

        private async Task<bool> HasAnyDataFiles(Guid releaseId)
        {
            return await _contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .AnyAsync(rf => rf.ReleaseId == releaseId && rf.File.Type == FileType.Data);
        }

        private async Task<PublicationType> GetPublicationType(Publication publication)
        {
            if (publication.LatestPublishedReleaseId == null)
            {
                return PublicationType.Legacy;
            }

            await _contentDbContext.Entry(publication)
                .Reference(p => p.LatestPublishedRelease)
                .LoadAsync();

            return GetPublicationType(publication.LatestPublishedRelease!.Type);
        }

        private static PublicationType GetPublicationType(ReleaseType releaseType) => releaseType switch
        {
            ReleaseType.AdHocStatistics => PublicationType.AdHoc,
            ReleaseType.NationalStatistics => PublicationType.NationalAndOfficial,
            ReleaseType.ExperimentalStatistics => PublicationType.Experimental,
            ReleaseType.ManagementInformation => PublicationType.ManagementInformation,
            ReleaseType.OfficialStatistics => PublicationType.NationalAndOfficial,
            _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, message: null)
        };
    }
}
