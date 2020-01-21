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

        public DownloadService(ContentDbContext context,
            IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        public IEnumerable<ThemeTree> GetTree(IEnumerable<Guid> includedReleaseIds)
        {
            var tree = _context.Themes
                .Include(theme => theme.Topics)
                .ThenInclude(topic => topic.Publications)
                .ThenInclude(publication => publication.Releases)
                .Select(BuildThemeTree)
                .Where(themeTree => themeTree.Topics.Any())
                .OrderBy(theme => theme.Title)
                .ToList();

            return tree;
        }

        private ThemeTree BuildThemeTree(Theme theme)
        {
            return new ThemeTree
            {
                Id = theme.Id,
                Title = theme.Title,
                Summary = theme.Summary,
                Topics = theme.Topics
                    .Select(BuildTopicTree)
                    .Where(topicTree => topicTree.Publications.Any())
                    .OrderBy(topic => topic.Title)
                    .ToList()
            };
        }

        private TopicTree BuildTopicTree(Topic topic)
        {
            return new TopicTree
            {
                Id = topic.Id,
                Title = topic.Title,
                Summary = topic.Summary,
                Publications = topic.Publications
                    .Where(publication => publication.Releases.Any(release => release.Live))
                    .Select(BuildPublicationTree)
                    .Where(publicationTree => publicationTree.DownloadFiles.Any())
                    .OrderBy(publication => publication.Title)
                    .ToList()
            };
        }

        private PublicationTree BuildPublicationTree(Publication publication)
        {
            return new PublicationTree
            {
                Id = publication.Id,
                Title = publication.Title,
                Summary = publication.Summary,
                Slug = publication.Slug,
                DownloadFiles = GetDownloadFiles(publication).ToList()
            };
        }

        private IEnumerable<FileInfo> GetDownloadFiles(Publication publication)
        {
            var mostRecentRelease = GetMostRecentRelease(publication);
            return _fileStorageService.ListPublicFiles(publication.Slug, mostRecentRelease.Slug);
        }

        private static Release GetMostRecentRelease(Publication publication)
        {
            return publication.Releases.OrderByDescending(release => release.Published).FirstOrDefault();
        }
    }
}