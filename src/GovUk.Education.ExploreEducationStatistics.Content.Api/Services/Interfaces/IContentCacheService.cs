using System.Threading.Tasks;

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