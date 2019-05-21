using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces
{
    public interface IReleaseService
    {
        Release GetRelease(string id);
        
        Release GetLatestRelease(string id);
    }
}