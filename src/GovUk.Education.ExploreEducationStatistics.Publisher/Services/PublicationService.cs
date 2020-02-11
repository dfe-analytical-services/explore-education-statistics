using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly ContentDbContext _context;
        private readonly IReleaseService _releaseService;
        private readonly IMapper _mapper;

        public PublicationService(ContentDbContext context, IMapper mapper, IReleaseService releaseService)
        {
            _context = context;
            _mapper = mapper;
            _releaseService = releaseService;
        }

        public async Task<CachedPublicationViewModel> GetViewModelAsync(Guid id, IEnumerable<Guid> includedReleaseIds)
        {
            var publication = await _context.Publications
                .Include(p => p.Contact)
                .Include(p => p.LegacyReleases)
                .Include(p => p.Topic)
                .ThenInclude(topic => topic.Theme)
                .SingleOrDefaultAsync(p => p.Id == id);

            var publicationViewModel = _mapper.Map<CachedPublicationViewModel>(publication);
            var latestRelease = _releaseService.GetLatestRelease(publication.Id, includedReleaseIds);
            publicationViewModel.LatestReleaseId = latestRelease.Id;
            publicationViewModel.Releases = GetReleaseViewModels(id, includedReleaseIds);
            return publicationViewModel;
        }

        public List<ThemeTree<PublicationTreeNode>> GetTree(IEnumerable<Guid> includedReleaseIds)
        {
            return _context.Themes
                .Include(theme => theme.Topics)
                .ThenInclude(topic => topic.Publications)
                .ThenInclude(publication => publication.Releases)
                .ToList()
                .Where(theme => IsThemePublished(theme, includedReleaseIds))
                .Select(theme => BuildThemeTree(theme, includedReleaseIds))
                .OrderBy(theme => theme.Title)
                .ToList();
        }

        public IEnumerable<Publication> ListPublicationsWithPublishedReleases()
        {
            return _context.Publications
                .Include(publication => publication.Releases)
                .ToList()
                .Where(publication =>
                    publication.Releases.Any(release => IsReleasePublished(release, Enumerable.Empty<Guid>())))
                .ToList();
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
                Summary = topic.Summary,
                Publications = topic.Publications
                    .Where(publication => IsPublicationPublished(publication, includedReleaseIds))
                    .Select(BuildPublicationNode)
                    .OrderBy(publication => publication.Title)
                    .ToList()
            };
        }

        private static PublicationTreeNode BuildPublicationNode(Publication publication)
        {
            return new PublicationTreeNode
            {
                Id = publication.Id,
                Title = publication.Title,
                Summary = publication.Summary,
                Slug = publication.Slug,
                LegacyPublicationUrl = publication.LegacyPublicationUrl?.ToString()
            };
        }

        private List<ReleaseTitleViewModel> GetReleaseViewModels(Guid publicationId,
            IEnumerable<Guid> includedReleaseIds)
        {
            var releases = _context.Releases
                .Where(release => release.PublicationId == publicationId)
                .ToList()
                .Where(release => IsReleasePublished(release, includedReleaseIds))
                .OrderBy(release => release.Year)
                .ThenBy(release => release.TimePeriodCoverage);
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
                   publication.Releases.Any(release => IsReleasePublished(release, includedReleaseIds));
        }

        private static bool IsReleasePublished(Release release, IEnumerable<Guid> includedReleaseIds)
        {
            return release.Live || includedReleaseIds.Contains(release.Id);
        }
    }
}