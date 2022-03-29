#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces
{
    public interface IMethodologyRepository
    {
        Task<List<Methodology>> GetByPublication(Guid publicationId);

        Task<Publication> GetOwningPublication(Guid methodologyId);

        Task<List<Guid>> GetAllPublicationIds(Guid methodologyId);

        Task<List<Methodology>> GetUnrelatedToPublication(Guid publicationId);
    }
}
