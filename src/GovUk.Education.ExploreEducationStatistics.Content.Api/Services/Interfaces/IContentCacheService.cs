using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces
{
    public interface IContentCacheService
    {
        Task<List<ThemeTree>> GetContentTreeAsync();

        Task<List<ThemeTree>> GetMethodologyTreeAsync();

        Task<List<ThemeTree>> GetDownloadTreeAsync();

        Task<Methodology> GetMethodologyAsync(string slug);
        
        Task<PublicationViewModel> GetPublicationAsync(string slug);
        
        Task<ReleaseViewModel> GetLatestReleaseAsync(string slug);

        Task<Release> GetReleaseAsync(string slug);


    }
}