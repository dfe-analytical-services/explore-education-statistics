using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class DownloadService : IDownloadService
    {
        private readonly ApplicationDbContext _context;

        public DownloadService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // TODO: Include the list of downloadable files
        public List<ThemeTree> GetTree()
        {
            var tree = _context.Themes.Select(t => new ThemeTree
            {
                Id = t.Id,
                Title = t.Title,
                Topics = t.Topics.Select(x => new TopicTree
                {
                    Id = x.Id,
                    Title = x.Title,
                    Summary = x.Summary,
                    Publications = x.Publications.Where(p => p.Releases.Any())
                        .Select(p => new PublicationTree
                        {
                            Id = p.Id,
                            Title = p.Title,
                            Summary = p.Summary,
                            Slug = p.Slug
                        })
                        .OrderBy(publication => publication.Title).ToList()
                }).Where(topic => topic.Publications.Any()).OrderBy(topic => topic.Title).ToList()
            }).Where(theme => theme.Topics.Any()).OrderBy(theme => theme.Title).ToList();

            return tree;
        }
    }
}
