using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseStatusRepository
    {
        Task<IEnumerable<ReleaseStatus>> GetAllByOverallStage(Guid releaseId, params ReleaseStatusOverallStage[] overallStages);
    }
}