using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces
{
    public interface IContentCacheService
    {
        Task<string> GetContentTreeAsync();

        Task<string> GetMethodologyTreeAsync();

        Task<string> GetDownloadTreeAsync();

        Task<string> GetMethodologyAsync(string slug);

        Task<string> GetPublicationAsync(string slug);

        Task<string> GetLatestReleaseAsync(string slug);

        Task<string> GetReleaseAsync(string publicationSlug, string releaseSlug);
    }
}