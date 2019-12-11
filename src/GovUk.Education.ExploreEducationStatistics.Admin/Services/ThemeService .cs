using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ThemeService : IThemeService
    {
        private readonly ContentDbContext _context;

        public ThemeService(ContentDbContext context)
        {
            _context = context;
        }

        public async Task<List<Theme>> GetUserThemesAsync(Guid userId)
        {
            var userTopics = await _context
                .UserReleaseRoles
                .Include(r => r.Release)
                .ThenInclude(release => release.Publication)
                .ThenInclude(publication => publication.Topic)
                .ThenInclude(topic => topic.Theme)
                .Where(r => r.UserId == userId)
                .Select(r => r.Release.Publication.Topic)
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
        
        public Task<ThemeSummaryViewModel> GetSummaryAsync(Guid id)
        { 
            return _context
                .Themes
                .Where(th => th.Id == id)
                .Select(th => new ThemeSummaryViewModel()
            {
                Id = th.Id,
                Title = th.Title
            })
                .FirstOrDefaultAsync();
        }
    }
}