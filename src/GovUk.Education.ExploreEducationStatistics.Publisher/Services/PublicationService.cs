using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Extensions.PublisherExtensions;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IReleaseService _releaseService;
        private readonly IMapper _mapper;

        public PublicationService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IMapper mapper,
            IReleaseService releaseService)
        {
            _contentDbContext = contentDbContext;
            _statisticsDbContext = statisticsDbContext;
            _mapper = mapper;
            _releaseService = releaseService;
        }

        public async Task<Publication> Get(Guid id)
        {
            return await _contentDbContext.Publications.FindAsync(id);
        }

        public async Task<CachedPublicationViewModel> GetViewModelAsync(Guid id, IEnumerable<Guid> includedReleaseIds)
        {
            var publication = await _contentDbContext.Publications
                .Include(p => p.Contact)
                .Include(p => p.LegacyReleases)
                .Include(p => p.Topic)
                .ThenInclude(topic => topic.Theme)
                .SingleOrDefaultAsync(p => p.Id == id);

            var publicationViewModel = _mapper.Map<CachedPublicationViewModel>(publication);
            var latestRelease = await _releaseService.GetLatestRelease(publication.Id, includedReleaseIds);
            publicationViewModel.LatestReleaseId = latestRelease.Id;
            publicationViewModel.Releases = GetReleaseViewModels(id, includedReleaseIds);
            return publicationViewModel;
        }

        public List<ThemeTree<PublicationTreeNode>> GetTree(IEnumerable<Guid> includedReleaseIds)
        {
            return _contentDbContext.Themes
                .Include(theme => theme.Topics)
                .ThenInclude(topic => topic.Publications)
                .ThenInclude(publication => publication.Releases)
                .ToList()
                .Where(theme => IsThemePublished(theme, includedReleaseIds))
                .Select(theme => BuildThemeTree(theme, includedReleaseIds))
                .OrderBy(theme => theme.Title)
                .ToList();
        }

        public List<Publication> GetPublicationsWithPublishedReleases()
        {
            return _contentDbContext.Publications
                .Include(publication => publication.Releases)
                .ToList()
                .Where(p => p.Releases.Any(release => release.IsLatestPublishedVersionOfRelease()))
                .ToList();
        }

        public async Task SetPublishedDate(Guid id, DateTime published)
        {
            var publication = await Get(id);

            if (publication == null)
            {
                throw new ArgumentException("Publication does not exist", nameof(id));
            }

            publication.Published = published;

            await UpdatePublication(publication);
        }

        private async Task UpdatePublication(Publication publication)
        {
            _contentDbContext.Update(publication);
            await _contentDbContext.SaveChangesAsync();

            // Synchronize the stats publication as well
            var statsPublication = await _statisticsDbContext.Publication
                .FindAsync(publication.Id);

            if (statsPublication == null)
            {
                var newStatsPublication = _mapper.Map(publication, new Data.Model.Publication());
                await _statisticsDbContext.Publication.AddAsync(newStatsPublication);
            }
            else
            {
                _mapper.Map(publication, statsPublication);
                _statisticsDbContext.Update(statsPublication);
            }

            await _statisticsDbContext.SaveChangesAsync();
        }

        private static ThemeTree<PublicationTreeNode> BuildThemeTree(Theme theme, IEnumerable<Guid> includedReleaseIds)
        {
            return new ThemeTree<PublicationTreeNode>
            {
                Id = theme.Id,
                Title = theme.Title,
                Summary = theme.Summary,
                Topics = theme.Topics.Where(topic => IsTopicPublished(topic, includedReleaseIds))
                    .Select(topic => BuildTopicTree(topic, includedReleaseIds))
                    .OrderBy(topic => topic.Title)
                    .ToList()
            };
        }

        private static TopicTree<PublicationTreeNode> BuildTopicTree(Topic topic, IEnumerable<Guid> includedReleaseIds)
        {
            return new TopicTree<PublicationTreeNode>
            {
                Id = topic.Id,
                Title = topic.Title,
                Publications = topic.Publications
                    .Where(publication => IsPublicationPublished(publication, includedReleaseIds))
                    .Select(publication => BuildPublicationNode(publication, includedReleaseIds))
                    .OrderBy(publication => publication.Title)
                    .ToList()
            };
        }

        private static PublicationTreeNode BuildPublicationNode(
            Publication publication,
            IEnumerable<Guid> includedReleaseIds)
        {
            // Ignore any legacyPublicationUrl once the Publication has Releases
            var legacyPublicationUrlIgnored =
                publication.Releases.Any(release => release.IsReleasePublished(includedReleaseIds));
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

        private List<ReleaseTitleViewModel> GetReleaseViewModels(
            Guid publicationId,
            IEnumerable<Guid> includedReleaseIds)
        {
            var releases = _contentDbContext.Releases
                .Include(r => r.Publication)
                .Where(release => release.PublicationId == publicationId)
                .ToList()
                .Where(release => release.IsReleasePublished(includedReleaseIds))
                .OrderByDescending(release => release.Year)
                .ThenByDescending(release => release.TimePeriodCoverage);
            return _mapper.Map<List<ReleaseTitleViewModel>>(releases);
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
            return !string.IsNullOrEmpty(publication.LegacyPublicationUrl?.ToString()) ||
                   publication.Releases.Any(release => release.IsReleasePublished(includedReleaseIds));
        }
    }
}
