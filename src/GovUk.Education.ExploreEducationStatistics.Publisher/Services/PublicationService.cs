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

        public List<ThemeTree> GetPublicationsTree()
        {
            return _context.Themes
                .Where(IsThemePublished)
                .Select(BuildThemeTree)
                .OrderBy(theme => theme.Title)
                .ToList();
        }

        public IEnumerable<Publication> ListPublicationsWithPublishedReleases()
        {
            return _context.Publications
                .Include(p => p.Releases)
                .ToList()
                .Where(IsPublicationPublished);
        }

        private static ThemeTree BuildThemeTree(Theme theme)
        {
            return new ThemeTree
            {
                Id = theme.Id,
                Title = theme.Title,
                Summary = theme.Summary,
                Topics = theme.Topics.Where(IsTopicPublished)
                    .Select(BuildTopicTree)
                    .OrderBy(topic => topic.Title)
                    .ToList()
            };
        }

        private static TopicTree BuildTopicTree(Topic topic)
        {
            return new TopicTree
            {
                Id = topic.Id,
                Title = topic.Title,
                Summary = topic.Summary,
                Publications = topic.Publications.Where(IsPublicationPublished)
                    .Select(BuildPublicationTree)
                    .OrderBy(publication => publication.Title)
                    .ToList()
            };
        }

        private static PublicationTree BuildPublicationTree(Publication p)
        {
            return new PublicationTree
            {
                Id = p.Id,
                Title = p.Title,
                Summary = p.Summary,
                Slug = p.Slug,
                LegacyPublicationUrl = p.LegacyPublicationUrl?.ToString()
            };
        }

        private static bool IsThemePublished(Theme theme)
        {
            return theme.Topics.Any(IsTopicPublished);
        }

        private static bool IsTopicPublished(Topic topic)
        {
            return topic.Publications.Any(IsPublicationPublished);
        }

        private static bool IsPublicationPublished(Publication publication)
        {
            return !string.IsNullOrEmpty(publication.LegacyPublicationUrl?.ToString()) ||
                   publication.Releases.Any(IsReleasePublished);
        }

        private static bool IsReleasePublished(Release release)
        {
            return release.Live;
        }
    }
}