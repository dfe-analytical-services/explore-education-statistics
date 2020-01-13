using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IThemeService
    {
        Task<List<Theme>> GetMyThemesAsync();

        Task<ThemeSummaryViewModel> GetSummaryAsync(Guid id);
    }
}