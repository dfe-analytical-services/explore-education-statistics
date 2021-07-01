using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces
{
    public interface IMethodologyRepository
    {
        Task<Methodology> CreateMethodologyForPublication(Guid publicationId);

        Task<List<Methodology>> GetLatestByPublication(Guid publicationId);

        Task<List<Methodology>> GetLatestPublishedByPublication(Guid publicationId);

        Task<bool> IsPubliclyAccessible(Guid methodologyId);
    }
}
