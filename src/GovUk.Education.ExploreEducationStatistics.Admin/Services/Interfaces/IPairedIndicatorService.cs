#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IPairedIndicatorService
{
    Task<Either<ActionResult, PairedIndicatorsViewModel>> GetPairedIndicators(
        Guid releaseVersionId,
        Guid subjectId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, PairedIndicatorsViewModel>> UpdatePairedIndicators(
        Guid releaseVersionId,
        Guid subjectId,
        PairedIndicatorsUpdateRequest request,
        CancellationToken cancellationToken = default
    );
}
