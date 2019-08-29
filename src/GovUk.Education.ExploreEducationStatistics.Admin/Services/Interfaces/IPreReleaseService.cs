using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPreReleaseService
    {
        Task<List<UserDetailsViewModel>> GetAvailablePreReleaseContactsAsync();
        
        Task<List<UserDetailsViewModel>> GetPreReleaseContactsForReleaseAsync(Guid releaseId);

        Task<List<UserDetailsViewModel>> AddPreReleaseContactToReleaseAsync(Guid releaseId, Guid userId);

        Task<List<UserDetailsViewModel>> RemovePreReleaseContactFromReleaseAsync(Guid releaseId, Guid userId);
    }
}
