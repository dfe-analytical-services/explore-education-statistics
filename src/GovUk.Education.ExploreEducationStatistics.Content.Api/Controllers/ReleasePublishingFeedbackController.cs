#nullable enable
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[Route("api/feedback/release-publishing")]
[ApiController]
public class ReleasePublishingFeedbackController(
    IReleasePublishingFeedbackService feedbackService) : ControllerBase
{
    [HttpPut]
    public async Task<ActionResult> UpdateFeedback(
        [FromBody] ReleasePublishingFeedbackUpdateRequest request,
        CancellationToken cancellationToken)
    {
        return await feedbackService
            .UpdateFeedback(request, cancellationToken)
            .HandleFailuresOrNoContent(convertNotFoundToNoContent: false);
    }
}
