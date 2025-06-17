using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services;

public class ReleasePublishingFeedbackService(
    ContentDbContext context,
    DateTimeProvider dateTimeProvider)
    : IReleasePublishingFeedbackService
{
    public Task<Either<ActionResult, Unit>> UpdateFeedback(
        ReleasePublishingFeedbackUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        return context
            .ReleasePublishingFeedback
            .SingleOrNotFoundAsync(
                feedback => feedback.Token == request.Token,
                cancellationToken)
            .OnSuccessDo(async feedback =>
            {
                feedback.Response = request.Response;
                feedback.AdditionalFeedback = request.AdditionalFeedback;
                feedback.FeedbackReceived = dateTimeProvider.UtcNow;
                await context.SaveChangesAsync(cancellationToken);
            })
            .OnSuccessVoid();
    }
}
