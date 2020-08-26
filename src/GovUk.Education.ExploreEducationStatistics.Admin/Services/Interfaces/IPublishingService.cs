using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPublishingService
    {
        Task<Either<ActionResult, Unit>> RetryReleaseStage(Guid releaseId, RetryStage stage);
        Task<Either<ActionResult, Unit>> ReleaseChanged(Guid id, bool immediate = false);
        Task<Either<ActionResult, Unit>> MethodologyChanged(Guid id);
        Task<Either<ActionResult, Unit>> PublicationChanged(Guid id);
    }
}