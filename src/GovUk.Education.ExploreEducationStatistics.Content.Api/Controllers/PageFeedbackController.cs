#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[Route("api/feedback/page")]
[ApiController]
public class PageFeedbackController(ContentDbContext context) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> CreateFeedback(
        [FromBody] PageFeedbackCreateRequest request,
        CancellationToken cancellationToken
    )
    {
        await context.PageFeedback.AddAsync(MapToEntity(request), cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new CreatedResult();
    }

    private static PageFeedback MapToEntity(PageFeedbackCreateRequest request)
    {
        return new PageFeedback
        {
            UserAgent = request.UserAgent,
            Url = request.Url,
            Response = request.Response,
            Context = request.Context,
            Intent = request.Intent,
            Issue = request.Issue,
        };
    }
}
