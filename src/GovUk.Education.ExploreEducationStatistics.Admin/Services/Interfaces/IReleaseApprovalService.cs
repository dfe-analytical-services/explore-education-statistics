#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseApprovalService
    {
        Task<Either<ActionResult, List<ReleaseStatusViewModel>>> GetReleaseStatuses(Guid releaseId);

        Task<Either<ActionResult, Unit>> CreateReleaseStatus(Guid releaseId, ReleaseStatusCreateRequest request);
    }
}
