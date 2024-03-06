#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs.Clients;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;

public class ContentBlockLockService : IContentBlockLockService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
    private readonly IUserService _userService;
    private readonly IHubContext<ReleaseContentHub, IReleaseContentHubClient> _hubContext;

    public ContentBlockLockService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IUserService userService,
        IHubContext<ReleaseContentHub, IReleaseContentHubClient> hubContext)
    {
        _contentDbContext = contentDbContext;
        _persistenceHelper = persistenceHelper;
        _userService = userService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Try to acquire the lock for a content block, notifying any
    /// subscribed hub clients if the lock was acquired.
    /// </summary>
    /// <returns>
    /// The lock details if was acquired. If another user
    /// holds the lock, then we return that instead.
    /// </returns>
    public async Task<Either<ActionResult, ContentBlockLockViewModel>> LockContentBlock(
        Guid id,
        bool force = false)
    {
        var userId = _userService.GetUserId();

        return await GetContentBlock(id)
            .OnSuccessDo(CheckCanUpdateBlock)
            .OnSuccess(
                async block =>
                {
                    if (TryFindConflictingLock(block, userId, force, out var conflictingLock))
                    {
                        return conflictingLock;
                    }

                    return await DoContentBlockLock(block, userId);
                }
            );
    }

    private async Task<ContentBlockLockViewModel> DoContentBlockLock(ContentBlock block, Guid userId)
    {
        var user = await _contentDbContext.Users.FindAsync(userId);

        if (user is null)
        {
            throw new ArgumentException($"User with id {userId} does not exist", nameof(userId));
        }

        var now = DateTime.UtcNow;

        block.Locked = now;
        block.LockedById = user.Id;

        _contentDbContext.ContentBlocks.Update(block);

        await _contentDbContext.SaveChangesAsync();

        var viewModel = new ContentBlockLockViewModel(
            Id: block.Id,
            SectionId: block.ContentSection!.Id,
            ReleaseVersionId: block.ContentSection!.ReleaseVersionId,
            Locked: now,
            LockedUntil: block.LockedUntil!.Value,
            LockedBy: new UserDetailsViewModel(user)
        );

        await _hubContext.Clients
            .Group(viewModel.ReleaseVersionId.ToString())
            .ContentBlockLocked(viewModel);

        return viewModel;
    }

    /// <summary>
    /// Try to unlock a content block, notifying any subscribed hub
    /// clients that the lock has been released.
    /// </summary>
    /// <returns>Nothing if the content block was successfully unlocked.</returns>
    public async Task<Either<ActionResult, Unit>> UnlockContentBlock(Guid id, bool force = false)
    {
        var userId = _userService.GetUserId();

        return await GetContentBlock(id)
            .OnSuccessDo(CheckCanUpdateBlock)
            .OnSuccessDo(block => CheckNoConflictingLock(block, userId, force))
            .OnSuccessVoid(
                async block =>
                {
                    block.Locked = null;
                    block.LockedById = null;

                    _contentDbContext.ContentBlocks.Update(block);

                    await _contentDbContext.SaveChangesAsync();

                    var viewModel = new ContentBlockUnlockViewModel(
                        Id: block.Id,
                        SectionId: block.ContentSection!.Id,
                        ReleaseVersionId: block.ContentSection!.ReleaseVersionId
                    );

                    await _hubContext.Clients
                        .Group(viewModel.ReleaseVersionId.ToString())
                        .ContentBlockUnlocked(viewModel);
                }
            );
    }

    private async Task<Either<ActionResult, ContentBlock>> GetContentBlock(Guid contentBlockId)
    {
        return await _persistenceHelper.CheckEntityExists<ContentBlock>(
            contentBlockId,
            q =>
                q.Include(block => block.ContentSection)
                 .Include(contentBlock => contentBlock.ReleaseVersion)
                 .Include(block => block.LockedBy)
        );
    }

    private async Task<Either<ActionResult, Unit>> CheckCanUpdateBlock(ContentBlock contentBlock)
    {
        if (contentBlock.ContentSection?.ReleaseVersion is null)
        {
            return new ForbidResult();
        }

        return await _userService
            .CheckCanUpdateRelease(contentBlock.ContentSection.ReleaseVersion)
            .OnSuccessVoid();
    }

    private bool TryFindConflictingLock(
        ContentBlock contentBlock,
        Guid userId,
        bool force,
        out ContentBlockLockViewModel conflictingLock)
    {
        conflictingLock = null!;

        if (force)
        {
            return false;
        }

        // Is not locked, or is locked by the current user (there is no conflict).
        if (contentBlock.LockedById == null || contentBlock.LockedById == userId)
        {
            return false;
        }

        // Is not locked, or lock has expired.
        if (!contentBlock.Locked.HasValue || contentBlock.LockedUntil <= DateTime.UtcNow)
        {
            return false;
        }

        // There is a conflict as another user has the lock.
        conflictingLock = new ContentBlockLockViewModel(
            Id: contentBlock.Id,
            SectionId: contentBlock.ContentSection!.Id,
            ReleaseVersionId: contentBlock.ContentSection!.ReleaseVersionId,
            Locked: contentBlock.Locked.Value,
            LockedUntil: contentBlock.LockedUntil!.Value,
            LockedBy: new UserDetailsViewModel(contentBlock.LockedBy!)
        );

        return true;
    }

    private Either<ActionResult, Unit> CheckNoConflictingLock(
        ContentBlock contentBlock,
        Guid userId,
        bool force)
    {
        if (TryFindConflictingLock(contentBlock, userId, force, out _))
        {
            return new ConflictResult();
        }

        return Unit.Instance;
    }
}
