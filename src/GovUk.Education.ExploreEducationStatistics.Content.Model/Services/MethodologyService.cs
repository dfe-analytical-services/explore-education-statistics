using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services
{
    public class MethodologyService : IMethodologyService
    {
        private readonly ApplicationDbContext _context;

        public MethodologyService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public Methodology Get(string slug)
        {
            var publication = _context.Publications.Include(p => p.Methodology).FirstOrDefault(p => p.Methodology != null && p.Slug == slug);
            return publication?.Methodology;
        }

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
                    Publications = x.Publications.Where(p => p.Methodology != null)
                        .Select(p => new PublicationTree
                        {
                            Id = p.Methodology.Id,
                            Title = p.Methodology.Title,
                            Summary = p.Methodology.Summary,
                            Slug = p.Slug
                        }).OrderBy(publication => publication.Title).ToList()
                }).Where(x => x.Publications.Any()).OrderBy(topic => topic.Title).ToList()
            }).Where(x => x.Topics.Any()).OrderBy(theme => theme.Title).ToList();

            return tree;
        }
    }
}
