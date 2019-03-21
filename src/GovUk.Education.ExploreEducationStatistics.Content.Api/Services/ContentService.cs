using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using Newtonsoft.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class ContentService : IContentService
    {
        private readonly ApplicationDbContext _context;

        public ContentService(ApplicationDbContext context)
        {
            _context = context;    

        }
        
        public List<ThemeTree> GetContentTree()
        {
            var tree = _context.Themes.Select(t => new ThemeTree { Id = t.Id,Title = t.Title, 
                Topics = t.Topics.Select(x => new TopicTree { Id = x.Id, Title = x.Title, Summary = x.Summary,
                    Publications = x.Publications.Select(p => new PublicationTree { Id = p.Id, Title = p.Title, Summary = p.Summary, Slug = p.Slug}).ToList()} ).ToList()}).ToList();

            return tree;
        }
    }

    public class ThemeTree
    {
        public Guid Id { get; set; }
        public string Title { get; set; }

        public List<TopicTree> Topics { get; set; }
    }

    public class TopicTree
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }

        public List<PublicationTree> Publications { get; set; }
    }
    
    public class PublicationTree
    
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Summary { get; set; }
    }
}