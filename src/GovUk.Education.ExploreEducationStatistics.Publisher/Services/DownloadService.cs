using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class DownloadService : IDownloadService
    {
        private readonly ContentDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly IReleaseService _releaseService;

        public DownloadService(ContentDbContext context,
            IFileStorageService fileStorageService,
            IReleaseService releaseService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _releaseService = releaseService;
        }

        public IEnumerable<ThemeTree> GetTree(IEnumerable<Guid> includedReleaseIds)
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

        private ThemeTree BuildThemeTree(Theme theme, IEnumerable<Guid> includedReleaseIds)
        {
            return new ThemeTree
            {
                Id = theme.Id,
                Title = theme.Title,
                Summary = theme.Summary,
                Topics = theme.Topics
                    .Where(topic => IsTopicPublished(topic, includedReleaseIds))
                    .Select(topic => BuildTopicTree(topic, includedReleaseIds))
                    .OrderBy(topic => topic.Title)
                    .ToList()
            };
        }

        private TopicTree BuildTopicTree(Topic topic, IEnumerable<Guid> includedReleaseIds)
        {
            return new TopicTree
            {
                Id = topic.Id,
                Title = topic.Title,
                Summary = topic.Summary,
                Publications = topic.Publications
                    .Where(publication => IsPublicationPublished(publication, includedReleaseIds))
                    .Select(publication => BuildPublicationTree(publication, includedReleaseIds))
                    .Where(publicationTree => publicationTree.DownloadFiles.Any())
                    .OrderBy(publication => publication.Title)
                    .ToList()
            };
        }

        private PublicationTree BuildPublicationTree(Publication publication, IEnumerable<Guid> includedReleaseIds)
        {
            return new PublicationTree
            {
                Id = publication.Id,
                Title = publication.Title,
                Summary = publication.Summary,
                Slug = publication.Slug,
                DownloadFiles = GetDownloadFiles(publication, includedReleaseIds).ToList()
            };
        }

        private IEnumerable<FileInfo> GetDownloadFiles(Publication publication, IEnumerable<Guid> includedReleaseIds)
        {
            var latestRelease = _releaseService.GetLatestRelease(publication.Id, includedReleaseIds);
            return _fileStorageService.ListPublicFiles(publication.Slug, latestRelease.Slug);
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