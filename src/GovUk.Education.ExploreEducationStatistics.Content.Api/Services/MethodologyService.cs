using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class MethodologyService : IMethodologyService
    {
        private readonly ApplicationDbContext _context;

        public MethodologyService(ApplicationDbContext context)
        {
            _context = context;
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
                    Publications = x.Publications
                        .Select(p => new PublicationTree
                        { Id = p.Id, Title = p.Title, Summary = p.Summary, Slug = p.Slug }).OrderBy(publication => publication.Title).ToList()
                }).OrderBy(topic => topic.Title).ToList()
            }).OrderBy(theme => theme.Title).ToList();

            return tree;
        }
    }
}
