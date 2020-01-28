using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly ContentDbContext _context;

        public PublicationService(ContentDbContext context)
        {
            _context = context;
        }

        public List<ThemeTree> GetTree(IEnumerable<Guid> includedReleaseIds)
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

        private static ThemeTree BuildThemeTree(Theme theme, IEnumerable<Guid> includedReleaseIds)
        {
            return new ThemeTree
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

        private static TopicTree BuildTopicTree(Topic topic, IEnumerable<Guid> includedReleaseIds)
        {
            return new TopicTree
            {
                Id = topic.Id,
                Title = topic.Title,
                Summary = topic.Summary,
                Publications = topic.Publications
                    .Where(publication => IsPublicationPublished(publication, includedReleaseIds))
                    .Select(BuildPublicationTree)
                    .OrderBy(publication => publication.Title)
                    .ToList()
            };
        }

        private static PublicationTree BuildPublicationTree(Publication publication)
        {
            return new PublicationTree
            {
                Id = publication.Id,
                Title = publication.Title,
                Summary = publication.Summary,
                Slug = publication.Slug,
                LegacyPublicationUrl = publication.LegacyPublicationUrl?.ToString()
            };
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