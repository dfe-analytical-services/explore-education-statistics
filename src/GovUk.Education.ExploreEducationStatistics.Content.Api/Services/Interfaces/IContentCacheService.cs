using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces
{
    public interface IContentCacheService
    {
        Task<List<ThemeTree>> GetContentTreeAsync();

        Task<List<ThemeTree>> GetMethodologyTreeAsync();

        Task<List<ThemeTree>> GetDownloadTreeAsync();

        Task<Methodology> GetMethodologyAsync(string slug);
    }
}