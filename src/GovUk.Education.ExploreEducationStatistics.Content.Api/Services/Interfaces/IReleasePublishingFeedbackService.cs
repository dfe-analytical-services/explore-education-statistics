using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;

public interface IReleasePublishingFeedbackService
{
    Task<Either<ActionResult, Unit>> UpdateFeedback(
        ReleasePublishingFeedbackUpdateRequest request,
        CancellationToken cancellationToken = default);
}
