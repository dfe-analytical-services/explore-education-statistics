using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces
{
    public interface IReleaseContentBlockRepository
    {
        public Task<List<T>> GetAll<T>(Guid releaseId) where T : ContentBlock;
    }
}
