using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

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

        public IEnumerable<ThemeTree> GetDownloadTree()
        {
            var tree = _context.Themes.Select(t => new ThemeTree
            {
                Id = t.Id, Title = t.Title, Summary = t.Summary,
                Topics = t.Topics.Select(x => new TopicTree
                {
                    Id = x.Id, Title = x.Title, Summary = x.Summary,
                    Publications = x.Publications.Where(pub => pub.Releases.Any())
                        .Select(p => new PublicationTree
                        {
                            Id = p.Id,
                            Title = p.Title,
                            Summary = p.Summary,
                            Slug = p.Slug,
                            DownloadFiles = _fileStorageService.ListPublicFiles(p.Slug, p.Releases.OrderByDescending(r => r.Published).FirstOrDefault().Slug).ToList()
                        }).Where(publication => publication.DownloadFiles.Any()).OrderBy(publication => publication.Title).ToList()
                }).Where(topic => topic.Publications.Any()).OrderBy(topic => topic.Title).ToList()
            }).Where(theme=> theme.Topics.Any()).OrderBy(theme => theme.Title).ToList();

            return tree;
        }
    }
}