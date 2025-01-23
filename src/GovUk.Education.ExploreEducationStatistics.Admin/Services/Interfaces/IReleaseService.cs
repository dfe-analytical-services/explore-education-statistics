#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IReleaseService
{
    Task<Either<ActionResult, ReleaseVersionViewModel>> CreateRelease(ReleaseCreateRequest releaseCreate);

    Task<Either<ActionResult, ReleaseViewModel>> UpdateRelease(Guid releaseId, ReleaseUpdateRequest releaseUpdate);
}
