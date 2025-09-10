#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[Route("api/feedback/release-publishing")]
[ApiController]
public class ReleasePublishingFeedbackController(
    ContentDbContext context,
    DateTimeProvider dateTimeProvider) : ControllerBase
{
    [HttpPut]
    public async Task<ActionResult> UpdateFeedback(
        [FromBody] ReleasePublishingFeedbackUpdateRequest request,
        CancellationToken cancellationToken)
    {
        return await context
            .ReleasePublishingFeedback
            .SingleOrNotFoundAsync(
                feedback => feedback.EmailToken == request.EmailToken,
                cancellationToken)
            .OnSuccessDo(async feedback =>
            {
                feedback.Response = request.Response;
                feedback.AdditionalFeedback = request.AdditionalFeedback;
                feedback.FeedbackReceived = dateTimeProvider.UtcNow;
                await context.SaveChangesAsync(cancellationToken);
            })
            .HandleFailuresOrNoContent(convertNotFoundToNoContent: false);
    }
}
