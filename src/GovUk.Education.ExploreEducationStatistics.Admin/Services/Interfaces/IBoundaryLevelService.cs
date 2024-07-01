#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IBoundaryLevelService
{
    Task<List<BoundaryLevelViewModel>> ListBoundaryLevels();

    Task<BoundaryLevelViewModel?> GetBoundaryLevel(long id);

    Task UpdateBoundaryLevel(
        long id,
        string label);
}
