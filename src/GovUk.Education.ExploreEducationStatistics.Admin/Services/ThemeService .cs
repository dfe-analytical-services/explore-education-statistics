using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using UserId = System.Guid;
namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ThemeService : IThemeService
    {
        private readonly ApplicationDbContext _context;

        public ThemeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Theme> GetThemes(UserId userId)
        {
            // TODO This method simply returns all Themes as we currently do not have a concept of how a user
            // TODO is connected to Themes for the purpose of administration. Once this has been modelled then
            // TODO this method will need altered reflect this. 
            var result = _context.Themes.Select(t => new Theme
            {
                Id = t.Id, Title = t.Title,
                Topics = t.Topics.Select(x => new Topic
                {
                    Id = x.Id, Title = x.Title
                }).OrderBy(topic => topic.Title).ToList()
            }).OrderBy(theme => theme.Title).ToList();

            return result;
        }
    }
}
