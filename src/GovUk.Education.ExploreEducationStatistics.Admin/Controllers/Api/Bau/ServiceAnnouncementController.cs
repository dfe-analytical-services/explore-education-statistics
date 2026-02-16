#nullable enable
using System.Security.Claims;
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
        var givenName = User.FindFirstValue(ClaimTypes.GivenName);
        var surname = User.FindFirstValue(ClaimTypes.Surname);
        var senderName = $"{givenName} {surname}";

        await hubContext
            .Clients.AllExcept(connectionId)
            .SendAsync("ServiceAnnouncement", senderName, message, cancellationToken);

        return Ok();
    }
}
