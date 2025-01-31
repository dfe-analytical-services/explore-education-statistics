#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs.Clients;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs.Messages;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Hubs;

[Authorize]
public class ReleaseContentHub : Hub<IReleaseContentHubClient>
{
    private readonly IContentBlockLockService _contentBlockLockService;

    public ReleaseContentHub(IContentBlockLockService contentBlockLockService)
    {
        _contentBlockLockService = contentBlockLockService;
    }

    public async Task JoinReleaseGroup(ReleaseMessage message)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, message.Id.ToString());
    }

    public async Task LeaveReleaseGroup(ReleaseMessage message)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, message.Id.ToString());
    }

    public async Task<HubResult<ContentBlockLockViewModel>> LockContentBlock(
        ContentBlockLockMessage lockMessage)
    {
        return await _contentBlockLockService
            .LockContentBlock(lockMessage.Id, lockMessage.Force)
            .HandleFailuresOrHubResult();
    }

    public async Task<HubResult> UnlockContentBlock(ContentBlockLockMessage lockMessage)
    {
        return await _contentBlockLockService
            .UnlockContentBlock(lockMessage.Id, lockMessage.Force)
            .HandleFailuresOrHubResult();
    }
}