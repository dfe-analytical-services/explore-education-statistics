using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseService
    {
        List<Release> List();

        Release Get(Guid id);

        Release Get(string slug);

        Task<ReleaseViewModel> CreateReleaseAsync(CreateReleaseViewModel release);

        Task<ReleaseViewModel> GetViewModel(ReleaseId id);
        
        Task<EditReleaseSummaryViewModel> GetReleaseSummaryAsync(ReleaseId releaseId);
        
        Task<ReleaseViewModel> EditReleaseSummaryAsync(EditReleaseSummaryViewModel model);
    }
}
