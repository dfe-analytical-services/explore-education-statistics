using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces
{
    public interface IMethodologyRepository
    {
        Task<List<Methodology>> GetLatestPublishedByPublication(Guid publicationId);
    }
}
