#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

[Route("api")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class PageFeedbackController(ContentDbContext context) : ControllerBase
{
    [HttpGet("feedback/page")]
    public async Task<ActionResult> ListFeedback(
        [FromQuery] bool showRead,
        CancellationToken cancellationToken
    )
    {
        var query = context.PageFeedback.OrderByDescending(x => x.Created).AsQueryable();

        if (!showRead)
        {
            query = query.Where(x => !x.Read);
        }

        var feedback = await query.Select(f => MapToViewModel(f)).ToListAsync(cancellationToken);

        return Ok(feedback);
    }

    [HttpPatch("feedback/page/{id:guid}")]
    public async Task<ActionResult> ToggleReadStatus(Guid id)
    {
        var feedback = await context.PageFeedback.FindAsync(id);

        if (feedback == null)
        {
            return NotFound();
        }

        feedback.Read = !feedback.Read;

        await context.SaveChangesAsync();

        return NoContent();
    }

    private static PageFeedbackViewModel MapToViewModel(PageFeedback entity)
    {
        return new PageFeedbackViewModel
        {
            Id = entity.Id,
            UserAgent = entity.UserAgent,
            Url = entity.Url,
            Created = entity.Created,
            Response = entity.Response.ToString(),
            Context = entity.Context,
            Intent = entity.Intent,
            Issue = entity.Issue,
            Read = entity.Read,
        };
    }
}
