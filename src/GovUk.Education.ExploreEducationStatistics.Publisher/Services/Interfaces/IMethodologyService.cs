using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IMethodologyService
    {
        Task<Methodology> Get(Guid id);

        Task<List<Methodology>> GetByRelease(Guid releaseId);

        Task<List<File>> GetFiles(Guid methodologyId, params FileType[] types);

        // TODO SOW4 EES-2378 Move to Content API
        List<ThemeTree<MethodologyTreeNode>> GetTree(IEnumerable<Guid> includedReleaseIds);
    }
}
