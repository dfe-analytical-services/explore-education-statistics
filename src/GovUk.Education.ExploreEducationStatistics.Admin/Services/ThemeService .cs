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

        public List<Theme> GetUserThemes(Guid userId)
        {
            // TODO This method simply returns all Themes as we currently do not have a concept of how a user
            // TODO is connected to Themes for the purpose of administration. Once this has been modelled then
            // TODO this method will need altered reflect this.
            return _context.Themes.Select(th => new Theme
            {
                Id = th.Id,
                Title = th.Title,
                Topics = th.Topics.Select(to => new Topic {Id = to.Id, Title = to.Title}).ToList()
            }).ToList();
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