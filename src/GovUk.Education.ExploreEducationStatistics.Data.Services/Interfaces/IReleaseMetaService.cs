using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface IReleaseMetaService
    {
        Task<IEnumerable<IdLabel>> GetSubjectsAsync(Guid releaseId);
    }
}