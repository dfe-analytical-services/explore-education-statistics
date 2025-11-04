#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IBoundaryLevelService
{
    Task<Either<ActionResult, List<BoundaryLevel>>> ListBoundaryLevels(CancellationToken cancellationToken = default);

    Task<Either<ActionResult, BoundaryLevel>> GetBoundaryLevel(long id, CancellationToken cancellationToken = default);

    Task<Either<ActionResult, BoundaryLevel>> CreateBoundaryLevel(
        GeographicLevel level,
        string label,
        DateTime published,
        IFormFile file,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, BoundaryLevel>> UpdateBoundaryLevel(
        long id,
        string label,
        CancellationToken cancellationToken = default
    );
}
