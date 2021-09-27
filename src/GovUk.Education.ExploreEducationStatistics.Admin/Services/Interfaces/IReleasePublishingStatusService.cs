using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleasePublishingStatusService
    {
        Task<Either<ActionResult, ReleasePublishingStatusViewModel>> GetReleaseStatusAsync(Guid releaseId);
    }
}