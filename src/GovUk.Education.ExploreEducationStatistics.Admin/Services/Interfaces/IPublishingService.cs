using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPublishingService
    {
        Task<Either<ActionResult, bool>> RetryStage(Guid releaseId, RetryStage stage);
        Task<Either<ActionResult, bool>> NotifyChange(Guid releaseId, bool immediate = false);
    }
}