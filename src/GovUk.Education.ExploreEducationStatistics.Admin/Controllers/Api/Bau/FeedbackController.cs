#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

[Route("api")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class FeedbackController(ContentDbContext context) : ControllerBase
{
    [HttpGet("feedback")]
    public async Task<ActionResult> ListFeedback(
        [FromQuery] bool showRead,
        CancellationToken cancellationToken)
    {
        var query = context.Feedback
            .OrderByDescending(x => x.Created)
            .AsQueryable();

        if (!showRead)
        {
            query = query.Where(x => !x.Read);
        }

        var feedback = await query
            .Select(f => MapToViewModel(f))
            .ToListAsync(cancellationToken);

        return Ok(feedback);
    }

    [HttpPatch("feedback/{id:guid}")]
    public async Task<ActionResult> ToggleReadStatus(Guid id)
    {
        var feedback = await context.Feedback.FindAsync(id);

        if (feedback == null)
        {
            return NotFound();
        }

        feedback.Read = !feedback.Read;

        await context.SaveChangesAsync();

        return NoContent();
    }

    private static FeedbackViewModel MapToViewModel(Feedback entity)
    {
        return new FeedbackViewModel
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
