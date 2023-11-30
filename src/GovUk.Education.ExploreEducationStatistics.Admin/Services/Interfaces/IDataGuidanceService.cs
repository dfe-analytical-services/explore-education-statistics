#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IDataGuidanceService
{
    public Task<Either<ActionResult, DataGuidanceViewModel>> GetDataGuidance(Guid releaseId,
        CancellationToken cancellationToken = default);

    public Task<Either<ActionResult, DataGuidanceViewModel>> UpdateDataGuidance(Guid releaseId,
        DataGuidanceUpdateRequest request,
        CancellationToken cancellationToken = default);

    public Task<Either<ActionResult, Unit>> ValidateForReleaseChecklist(Guid releaseId,
        CancellationToken cancellationToken = default);
}
