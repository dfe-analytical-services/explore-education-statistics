using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies
{
    public interface IMethodologyRepository
    {
        Task<bool> UserHasReleaseRoleAssociatedWithMethodology(
            Guid userId,
            Guid methodologyId);

        Task<List<Methodology>> GetMethodologiesForUser(Guid userId);
    }
}
