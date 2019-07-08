using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
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
            return _context.Methodologies.Include(x => x.Publication).FirstOrDefault(x => x.Publication.Slug == slug);
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
                    Publications = x.Publications.Where(p => p.Methodologies.Any())
                        .Select(p => new PublicationTree
                        {
                            Id = p.Methodologies.FirstOrDefault().Id,
                            Title = p.Methodologies.FirstOrDefault().Title,
                            Summary = p.Methodologies.FirstOrDefault().Summary,
                            Slug = p.Slug
                        }).OrderBy(publication => publication.Title).ToList()
                }).Where(x => x.Publications.Any()).OrderBy(topic => topic.Title).ToList()
            }).Where(x => x.Topics.Any()).OrderBy(theme => theme.Title).ToList();

            return tree;
        }
    }
}
