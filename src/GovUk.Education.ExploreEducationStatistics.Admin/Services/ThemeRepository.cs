using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ThemeRepository : IThemeRepository
    {
        private readonly ContentDbContext _context;

        public ThemeRepository(ContentDbContext context)
        {
            _context = context;
        }
        
        public async Task<List<Theme>> GetAllThemesAsync()
        {
            return await _context.Themes.Select(th => new Theme
            {
                Id = th.Id,
                Title = th.Title,
                Topics = th.Topics.Select(to => new Topic {Id = to.Id, Title = to.Title}).ToList()
            }).ToListAsync();
        }

        public async Task<List<Theme>> GetThemesRelatedToUserAsync(Guid userId)
        {
            var userTopics = await _context
                .UserReleaseRoles
                .Include(r => r.Release)
                .ThenInclude(release => release.Publication)
                .ThenInclude(publication => publication.Topic)
                .ThenInclude(topic => topic.Theme)
                .Where(r => r.UserId == userId)
                .Select(r => r.Release.Publication.Topic)
                .Distinct()
                .ToListAsync();

            var userTopicsByTheme = new Dictionary<Theme, List<Topic>>();
            
            foreach (var theme in userTopics.Select(topic => topic.Theme).Distinct())
            {
                var topicsForTheme = userTopics.FindAll(topic => topic.Theme == theme);
                userTopicsByTheme.Add(theme, topicsForTheme);
            }
            
            return userTopicsByTheme
                .Select(themeAndTopics => 
                    new Theme
                    {
                        Id = themeAndTopics.Key.Id,
                        Title = themeAndTopics.Key.Title,
                        Topics = themeAndTopics
                            .Value
                            .Select(to => 
                                new Topic
                                {
                                    Id = to.Id, 
                                    Title = to.Title
                                })
                            .ToList()
                    })
                .ToList();
        }
    }
}