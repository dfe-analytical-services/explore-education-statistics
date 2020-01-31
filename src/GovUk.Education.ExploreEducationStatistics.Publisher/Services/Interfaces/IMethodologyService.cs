using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IMethodologyService
    {
        Task<MethodologyViewModel> GetViewModelAsync(Guid id);
        List<ThemeTree> GetTree(IEnumerable<Guid> includedReleaseIds);
    }
}