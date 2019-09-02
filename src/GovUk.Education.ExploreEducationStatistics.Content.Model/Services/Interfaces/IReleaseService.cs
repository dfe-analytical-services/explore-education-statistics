using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces
{
    public interface IReleaseService
    {
        ReleaseViewModel GetRelease(Guid id);
        
        ReleaseViewModel GetLatestRelease(Guid id);
    }
}