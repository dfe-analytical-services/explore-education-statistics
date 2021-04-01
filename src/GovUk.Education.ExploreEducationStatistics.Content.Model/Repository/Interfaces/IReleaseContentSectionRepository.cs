using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces
{
    public interface IReleaseContentSectionRepository
    {
        public Task<List<T>> GetAllContentBlocks<T>(Guid releaseId) where T : ContentBlock;
    }
}
