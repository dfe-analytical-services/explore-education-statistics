#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FeedbackController(ContentDbContext context) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> CreateFeedback(
            [FromBody] FeedbackCreateRequest request,
            CancellationToken cancellationToken)
    {
        await context.Feedback.AddAsync(MapToEntity(request), cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new CreatedResult();
    }

    private static Feedback MapToEntity(FeedbackCreateRequest request)
    {
        return new Feedback
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
