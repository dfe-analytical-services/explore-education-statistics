using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces
{
    public interface IReleaseService
    {
        Release GetRelease(string id);
        
        ReleaseViewModel GetLatestRelease(string id);
    }
}