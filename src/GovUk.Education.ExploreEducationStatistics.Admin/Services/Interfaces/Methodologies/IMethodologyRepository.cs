using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies
{
    public interface IMethodologyRepository
    {
        Task<List<Methodology>> GetLatestByPublication(Guid publicationId);

        Task<Methodology> CreateMethodologyForPublication(Guid publicationId);
    }
}
