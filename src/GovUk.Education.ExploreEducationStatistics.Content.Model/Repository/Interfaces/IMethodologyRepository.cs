#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces
{
    public interface IMethodologyRepository
    {
        public Task<List<Methodology>> GetByPublication(Guid publicationId);

        Task<Publication> GetOwningPublication(Guid methodologyId);

        public Task<List<Methodology>> GetUnrelatedToPublication(Guid publicationId);
    }
}
