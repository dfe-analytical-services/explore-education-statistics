using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IBoundaryLevelService
{
    Task<List<BoundaryLevelViewModel>> Get();

    Task<BoundaryLevelViewModel> Get(long id);

    Task UpdateLabel(
        long id,
        string label);
}
