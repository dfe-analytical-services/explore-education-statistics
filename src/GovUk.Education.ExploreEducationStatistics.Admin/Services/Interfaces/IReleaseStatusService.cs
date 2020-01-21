using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseStatusService
    {
        Task<Either<ActionResult, IEnumerable<ReleaseStatusViewModel>>> GetReleaseStatusesAsync(Guid releaseId);
    }
}