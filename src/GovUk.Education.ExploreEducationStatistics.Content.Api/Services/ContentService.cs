using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Newtonsoft.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class ContentService : IContentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        
        public ContentService(
            ApplicationDbContext context,
            IMapper _mapper)
        {
            _context = context;
            _mapper = _mapper;
        }

        public List<ThemeTree> GetContentTree()
        {
            var tree = _context.Themes.Select(t => new ThemeTree
            {
                Id = t.Id, Title = t.Title,
                Topics = t.Topics.Select(x => new TopicTree
                {
                    Id = x.Id, Title = x.Title, Summary = x.Summary,
                    Publications = x.Publications
                        .Where(p => p.Releases.Count > 0)
                        .Select(p => new PublicationTree
                            {Id = p.Id, Title = p.Title, Summary = p.Summary, Slug = p.Slug}).OrderBy(publication => publication.Title).ToList()
                }).OrderBy(topic => topic.Title).ToList()
            }).OrderBy(theme => theme.Title).ToList();

            return tree;
        }
    }
}