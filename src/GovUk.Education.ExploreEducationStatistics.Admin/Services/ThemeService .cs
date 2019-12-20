using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ThemeService : IThemeService
    {
        private readonly ContentDbContext _context;
        private readonly IUserService _userService;

        public ThemeService(ContentDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public Task<List<Theme>> GetMyThemesAsync()
        {
            return _userService
                .CheckCanViewAllTopics()
                .OnSuccess(GetAllThemesAsync)
                .OrElse(GetThemesRelatedToUserAsync);
        }

        private async Task<List<Theme>> GetAllThemesAsync()
        {
            return await _context.Themes.Select(th => new Theme
            {
                Id = th.Id,
                Title = th.Title,
                Topics = th.Topics.Select(to => new Topic {Id = to.Id, Title = to.Title}).ToList()
            }).ToListAsync();
        }

        private async Task<List<Theme>> GetThemesRelatedToUserAsync()
        {
            var userId = _userService.GetUserId();
            
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