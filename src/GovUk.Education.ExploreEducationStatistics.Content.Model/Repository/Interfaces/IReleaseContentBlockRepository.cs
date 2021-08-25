using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces
{
    public interface IReleaseContentBlockRepository
    {
        Task<List<T>> GetByRelease<T>(Guid releaseId) where T : ContentBlock;
    }
}
