using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ContentService : IContentService
    {
        private readonly ContentDbContext _context;

        public ContentService(
            ContentDbContext context)
        {
            _context = context;
        }

        public List<ThemeTree> GetContentTree()
        {
            var tree = _context.Themes.Select(t => new ThemeTree
            {
                Id = t.Id, Title = t.Title, Summary = t.Summary,
                Topics = t.Topics.Select(x => new TopicTree
                {
                    Id = x.Id, Title = x.Title, Summary = x.Summary,
                    Publications = x.Publications
                        .Select(p => new PublicationTree
                        {
                            Id = p.Id,
                            Title = p.Title,
                            Summary = p.Summary,
                            Slug = p.Slug,
                            LegacyPublicationUrl = p.LegacyPublicationUrl != null ? p.LegacyPublicationUrl.ToString() : null
                        }).OrderBy(publication => publication.Title).ToList()
                }).OrderBy(topic => topic.Title).ToList()
            }).OrderBy(theme => theme.Title).ToList();

            return tree;
        }
    }
}