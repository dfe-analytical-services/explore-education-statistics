#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Releases.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Releases;

public class ReleaseUpdatesService(ContentDbContext contentDbContext, IUserService userService) : IReleaseUpdatesService
{
    public Task<Either<ActionResult, List<ReleaseUpdateDto>>> CreateReleaseUpdate(
        Guid releaseVersionId,
        DateTime? date,
        string reason,
        CancellationToken cancellationToken = default
    ) =>
        contentDbContext
            .ReleaseVersions.SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                var update = new Update
                {
                    // TODO EES-6490 Convert 'On' from DateTime to DateTimeOffset
                    On = date ?? DateTime.Now,
                    Reason = reason,
                    ReleaseVersionId = releaseVersion.Id,
                    CreatedById = userService.GetUserId(),
                };

                contentDbContext.Update.Add(update);
                await contentDbContext.SaveChangesAsync(cancellationToken);

                return await GetReleaseNoteViewModels(releaseVersion.Id, cancellationToken);
            });

    public Task<Either<ActionResult, PaginatedListViewModel<ReleaseUpdateDto>>> GetReleaseUpdates(
        Guid releaseVersionId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    ) =>
        contentDbContext
            .ReleaseVersions.SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken)
            .OnSuccess(userService.CheckCanViewReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                var updates = await contentDbContext
                    .Update.AsNoTracking()
                    .Where(u => u.ReleaseVersionId == releaseVersion.Id)
                    .Select(u => ReleaseUpdateDto.FromModel(u))
                    .ToListAsync(cancellationToken);

                var firstPublishedUpdate = new ReleaseUpdateDto
                {
                    Id = Guid.Empty,
                    Date = DateTime.Now, //await GetReleaseFirstPublishedDate(releaseVersion),
                    Summary = "First published",
                };

                var allUpdates = updates.Append(firstPublishedUpdate).OrderByDescending(u => u.Date).ToList();

                // Pagination is applied in-memory since the 'First published' entry is combined with database results
                return PaginatedListViewModel<ReleaseUpdateDto>.Paginate(allUpdates, page, pageSize);
            });

    public Task<Either<ActionResult, List<ReleaseUpdateDto>>> UpdateReleaseUpdate(
        Guid releaseVersionId,
        Guid releaseNoteId,
        DateTime? date,
        string reason,
        CancellationToken cancellationToken = default
    ) =>
        contentDbContext
            .ReleaseVersions.SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
                await contentDbContext.Update.SingleOrNotFoundAsync(
                    u => u.Id == releaseNoteId && u.ReleaseVersionId == releaseVersion.Id,
                    cancellationToken
                )
            )
            .OnSuccess(async update =>
            {
                // TODO EES-6490 Convert 'On' from DateTime to DateTimeOffset
                update.On = date ?? DateTime.Now;
                update.Reason = reason;
                await contentDbContext.SaveChangesAsync(cancellationToken);
                return await GetReleaseNoteViewModels(releaseVersionId, cancellationToken);
            });

    public Task<Either<ActionResult, List<ReleaseUpdateDto>>> DeleteReleaseUpdate(
        Guid releaseVersionId,
        Guid releaseNoteId,
        CancellationToken cancellationToken = default
    ) =>
        contentDbContext
            .ReleaseVersions.SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
                await contentDbContext.Update.SingleOrNotFoundAsync(
                    u => u.Id == releaseNoteId && u.ReleaseVersionId == releaseVersion.Id,
                    cancellationToken
                )
            )
            .OnSuccess(async update =>
            {
                contentDbContext.Update.Remove(update);
                await contentDbContext.SaveChangesAsync(cancellationToken);
                return await GetReleaseNoteViewModels(releaseVersionId, cancellationToken);
            });

    private Task<List<ReleaseUpdateDto>> GetReleaseNoteViewModels(
        Guid releaseVersionId,
        CancellationToken cancellationToken
    ) =>
        contentDbContext
            .Update.AsNoTracking()
            .Where(u => u.ReleaseVersionId == releaseVersionId)
            .OrderByDescending(u => u.On)
            .Select(u => ReleaseUpdateDto.FromModel(u))
            .ToListAsync(cancellationToken: cancellationToken);
}
