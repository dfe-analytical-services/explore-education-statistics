using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IMethodologyService
    {
        Task<MethodologyViewModel> GetViewModelAsync(Guid id, PublishContext context);
        List<ThemeTree<MethodologyTreeNode>> GetTree(IEnumerable<Guid> includedReleaseIds);
    }
}