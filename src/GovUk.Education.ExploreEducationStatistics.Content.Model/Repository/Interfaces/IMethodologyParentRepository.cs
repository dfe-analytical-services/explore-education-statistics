#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces
{
    public interface IMethodologyParentRepository
    {
        public Task<List<MethodologyParent>> GetByPublication(Guid publicationId);

        public Task<List<MethodologyParent>> GetUnrelatedToPublication(Guid publicationId);
    }
}
