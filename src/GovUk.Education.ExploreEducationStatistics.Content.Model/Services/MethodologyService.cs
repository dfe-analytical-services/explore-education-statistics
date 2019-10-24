using System;
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
        private readonly ContentDbContext _context;

        public MethodologyService(
            ContentDbContext context)
        {
            _context = context;
        }

        public Methodology Get(Guid id)
        {
            return _context.Methodologies.FirstOrDefault(p => p.Id == id);
        }
        
        public IEnumerable<Methodology> Get()
        {
            return _context.Methodologies.ToList();
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
                    Publications = x.Publications.Where(p => p.Methodology != null && p.Releases.Any(r => r.Published != null))
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