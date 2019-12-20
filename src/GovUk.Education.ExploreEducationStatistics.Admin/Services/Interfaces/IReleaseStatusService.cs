using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseStatusService
    {
        Task<IEnumerable<ReleaseStatusViewModel>> GetReleaseStatusesAsync(Guid releaseId);
    }
}