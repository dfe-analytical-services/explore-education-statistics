using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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

        public List<Theme> GetUserThemes(UserId userId)
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
    }
}