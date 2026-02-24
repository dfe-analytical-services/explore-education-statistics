#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

[Route("api")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class ServiceAnnouncementController(IHubContext<NotificationHub> hubContext) : ControllerBase
{
    [HttpPost("broadcastMessage")]
    public async Task<ActionResult> BroadcastMessage(
        [FromForm] string message,
        [FromForm] string connectionId,
        CancellationToken cancellationToken
    )
    {
        await hubContext.Clients.AllExcept(connectionId).SendAsync("ServiceAnnouncement", message, cancellationToken);

        return Ok();
    }
}
