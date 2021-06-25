using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IMethodologyService
    {
        Task<Methodology> Get(Guid id);

        Task<List<Methodology>> GetLatestByRelease(Guid releaseId);

        Task<List<File>> GetFiles(Guid methodologyId, params FileType[] types);
    }
}
