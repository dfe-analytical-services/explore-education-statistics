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
        private readonly IThemeRepository _repository;

        public ThemeService(ContentDbContext context, IUserService userService, IThemeRepository repository)
        {
            _context = context;
            _userService = userService;
            _repository = repository;
        }

        public Task<List<Theme>> GetMyThemesAsync()
        {
            return _userService
                .CheckCanViewAllTopics()
                .OnSuccess(() => _repository.GetAllThemesAsync())
                .OrElse(() => _repository.GetThemesRelatedToUserAsync(_userService.GetUserId()));
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