#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IReleaseAmendmentService
{
    Task<Either<ActionResult, IdViewModel>> CreateReleaseAmendment(Guid releaseVersionId);
}
