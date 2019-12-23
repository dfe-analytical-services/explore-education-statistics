using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IThemeRepository
    {
        Task<List<Theme>> GetAllThemesAsync();

        Task<List<Theme>> GetThemesRelatedToUserAsync(Guid userId);
    }
}