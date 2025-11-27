#nullable enable
using Cronos;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.CronExpressionUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseApprovalService(
    ContentDbContext context,
    TimeProvider timeProvider,
    IUserService userService,
    IPublishingService publishingService,
    IReleaseChecklistService releaseChecklistService,
    IContentService contentService,
    IUserResourceRoleNotificationService userResourceRoleNotificationService,
    IReleaseFileRepository releaseFileRepository,
    IReleaseFileService releaseFileService,
    IOptions<ReleaseApprovalOptions> options,
    IUserReleaseRoleService userReleaseRoleService,
    IUserPublicationRoleRepository userPublicationRoleRepository,
    IEmailTemplateService emailTemplateService
) : IReleaseApprovalService
{
    public async Task<Either<ActionResult, List<ReleaseStatusViewModel>>> ListReleaseStatuses(Guid releaseVersionId)
    {
        return await context
            .ReleaseVersions.SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId)
            .OnSuccess(userService.CheckCanViewReleaseVersionStatusHistory)
            .OnSuccess(async releaseVersion =>
            {
                return await context
                    .ReleaseStatus.Where(rs => rs.ReleaseVersion.ReleaseId == releaseVersion.ReleaseId)
                    .OrderByDescending(rs => rs.Created)
                    .Select(rs => new ReleaseStatusViewModel
                    {
                        ReleaseStatusId = rs.Id,
                        InternalReleaseNote = rs.InternalReleaseNote,
                        ApprovalStatus = rs.ApprovalStatus,
                        Created = rs.Created,
                        CreatedByEmail = rs.CreatedBy == null ? null : rs.CreatedBy.Email,
                        ReleaseVersion = rs.ReleaseVersion.Version,
                    })
                    .ToListAsync();
            });
    }

    public async Task<Either<ActionResult, Unit>> CreateReleaseStatus(
        Guid releaseVersionId,
        ReleaseStatusCreateRequest request
    )
    {
        return await context
            .ReleaseVersions.HydrateReleaseForChecklist()
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId)
            .OnSuccessDo(releaseVersion =>
                userService.CheckCanUpdateReleaseVersionStatus(releaseVersion, request.ApprovalStatus)
            )
            .OnSuccessDo(() => ValidatePublishDate(request))
            .OnSuccessDo(() => RemoveUnusedImages(releaseVersionId))
            .OnSuccess(async releaseVersion =>
            {
                if (request.ApprovalStatus != ReleaseApprovalStatus.Approved && releaseVersion.Published.HasValue)
                {
                    return ValidationActionResult(PublishedReleaseCannotBeUnapproved);
                }

                var oldStatus = releaseVersion.ApprovalStatus;

                releaseVersion.ApprovalStatus = request.ApprovalStatus;
                releaseVersion.NextReleaseDate = request.NextReleaseDate;
                releaseVersion.NotifySubscribers = releaseVersion.Version == 0 || (request.NotifySubscribers ?? false);
                releaseVersion.UpdatePublishedDate = request.UpdatePublishedDate ?? false;
                releaseVersion.PublishScheduled =
                    request.PublishMethod == PublishMethod.Immediate
                        ? timeProvider.GetUtcNow()
                        : request.PublishScheduled?.GetUkStartOfDayUtc();

                var releaseStatus = new ReleaseStatus
                {
                    Id = Guid.NewGuid(),
                    ReleaseVersion = releaseVersion,
                    InternalReleaseNote = request.InternalReleaseNote,
                    ApprovalStatus = request.ApprovalStatus,
                    CreatedById = userService.GetUserId(),
                };

                var releasePublishingKey = new ReleasePublishingKey(releaseVersionId, releaseStatus.Id);

                return await ValidateReleaseWithChecklist(releaseVersion)
                    .OnSuccess(() =>
                        SendEmailNotificationsAndInvites(request, releaseVersion)
                            .OnSuccess(() => NotifyPublisher(releasePublishingKey, request, oldStatus))
                            .OnSuccessDo(async () =>
                            {
                                context.ReleaseVersions.Update(releaseVersion);
                                await context.AddAsync(releaseStatus);
                                await context.SaveChangesAsync();
                            })
                    );
            });
    }

    private async Task<Either<ActionResult, Unit>> NotifyPublisher(
        ReleasePublishingKey releasePublishingKey,
        ReleaseStatusCreateRequest request,
        ReleaseApprovalStatus oldStatus
    )
    {
        // Only need to inform Publisher if changing release approval status to or from Approved
        if (oldStatus == ReleaseApprovalStatus.Approved || request.ApprovalStatus == ReleaseApprovalStatus.Approved)
        {
            return await publishingService.ReleaseChanged(
                releasePublishingKey,
                immediate: request.PublishMethod == PublishMethod.Immediate
            );
        }

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, Unit>> SendEmailNotificationsAndInvites(
        ReleaseStatusCreateRequest request,
        ReleaseVersion releaseVersion
    )
    {
        if (
            request.ApprovalStatus == ReleaseApprovalStatus.Approved
            && request.PublishMethod == PublishMethod.Scheduled
        )
        {
            await SendPreReleaseUserInviteEmails(releaseVersion);

            return Unit.Instance;
        }

        if (request.ApprovalStatus == ReleaseApprovalStatus.HigherLevelReview)
        {
            return await SendHigherLevelReviewNotificationEmails(releaseVersion);
        }

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, Unit>> SendHigherLevelReviewNotificationEmails(
        ReleaseVersion releaseVersion
    )
    {
        var userReleaseRoles = await userReleaseRoleService.ListLatestUserReleaseRolesByPublication(
            publicationId: releaseVersion.Release.PublicationId,
            rolesToInclude: [ReleaseRole.Approver]
        );

        var userPublicationRoles = await userPublicationRoleRepository.ListRolesForPublication(
            publicationId: releaseVersion.Release.PublicationId,
            rolesToInclude: [PublicationRole.Allower]
        );

        var notifyHigherReviewers = userReleaseRoles.Any() || userPublicationRoles.Any();

        if (notifyHigherReviewers)
        {
            return userReleaseRoles
                .Select(urr => urr.User.Email)
                .Concat(userPublicationRoles.Select(upr => upr.User.Email))
                .Distinct()
                .Select(email => emailTemplateService.SendReleaseHigherReviewEmail(email, releaseVersion))
                .OnSuccessAllReturnVoid();
        }

        return Unit.Instance;
    }

    private async Task SendPreReleaseUserInviteEmails(ReleaseVersion releaseVersion)
    {
        var unsentUserReleaseRoleInvites = await context
            .UserReleaseRoles.Where(urr => urr.ReleaseVersionId == releaseVersion.Id)
            .Where(urr => urr.Role == ReleaseRole.PrereleaseViewer)
            .Where(urr => urr.EmailSent == null)
            .ToListAsync();

        // This isn't ideal currently.
        // If one email fails to send, this code will stop sending further emails and the calling code will not notify
        // the Publisher and the release status will remain unchanged. This means some people will get an email saying they
        // now have pre-release access to a Release which isn't approved yet.
        // On the other hand, if we skip over failed emails and continue sending the rest, some people will not get notified
        // of their access even when the Release is now approved. This makes them hard to detect.
        await unsentUserReleaseRoleInvites
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async invite =>
                await userResourceRoleNotificationService.NotifyUserOfNewPreReleaseRole(
                    userId: invite.UserId,
                    releaseVersionId: releaseVersion.Id
                )
            );
    }

    private async Task<Either<ActionResult, Unit>> RemoveUnusedImages(Guid releaseVersionId)
    {
        return await contentService
            .GetContentBlocks<HtmlBlock>(releaseVersionId)
            .OnSuccess(async contentBlocks =>
            {
                var contentImageIds = contentBlocks
                    .SelectMany(contentBlock => HtmlImageUtil.GetReleaseImages(contentBlock.Body))
                    .Distinct();

                var imageFiles = await releaseFileRepository.GetByFileType(releaseVersionId, types: FileType.Image);

                var unusedImages = imageFiles
                    .Where(file => !contentImageIds.Contains(file.File.Id))
                    .Select(file => file.File.Id)
                    .ToList();

                if (unusedImages.Any())
                {
                    return await releaseFileService.Delete(
                        releaseVersionId: releaseVersionId,
                        fileIds: unusedImages,
                        forceDelete: true
                    );
                }

                return Unit.Instance;
            });
    }

    private Either<ActionResult, Unit> ValidatePublishDate(ReleaseStatusCreateRequest request)
    {
        if (
            request.ApprovalStatus == ReleaseApprovalStatus.Approved
            && request.PublishMethod == PublishMethod.Scheduled
        )
        {
            // Publish date must be set
            if (request.PublishScheduled == null)
            {
                return ValidationActionResult(PublishDateCannotBeEmpty);
            }

            // Check if publishing will occur on the publish date as there may be no scheduled occurrences
            // of the two Azure Functions which perform publishing.
            if (!CheckPublishDateCanBeScheduled(request.PublishScheduled.Value))
            {
                return ValidationActionResult(PublishDateCannotBeScheduled);
            }
        }

        return Unit.Instance;
    }

    private bool CheckPublishDateCanBeScheduled(DateOnly publishDate)
    {
        // Publishing a scheduled release relies on two Azure Functions which are triggered by cron expressions.
        // These notes will refer to them as functions (1) and (2):
        // StageScheduledReleases (1) - Runs tasks for the releases that are scheduled to be published.
        // PublishScheduledReleases (2) - Runs after (1) and completes publishing of releases.

        // The cron expressions are configurable per environment to allow different schedules for testing.

        // There's a requirement that (1) and (2) always run at specific times in the Prod environment,
        // e.g. (1) runs at 00:00:00 and (2) runs at 09:30:00 irrespective of daylight saving time.
        // To avoid needing to adjust a cron expression for daylight saving time twice a year to express
        // the desired time in UTC (e.g. changing it from '0 30 9 * * *' to '0 30 8 * * *' in British Summer Time),
        // the functions have been configured to run in the UK timezone rather than the UTC default.

        // Check if the functions have a scheduled occurrence on the publish date in the range between midnight
        // (inclusive) and midnight the following day (exclusive), UK time.

        var ukTimeZone = TimeZoneUtils.GetUkTimeZone();
        var fromUtc = publishDate.GetUkStartOfDayUtc();
        var toUtc = fromUtc.AddDays(1).AddTicks(-1);

        // The publish date cannot be scheduled if it's already passed
        var nowUtc = timeProvider.GetUtcNow();

        if (nowUtc > toUtc)
        {
            return false;
        }

        // The range should begin now rather than at midnight if the publish date is today
        if (nowUtc > fromUtc)
        {
            fromUtc = nowUtc;
        }

        // Publishing won't occur unless there's an occurrence of (1) between the publishing range
        var nextOccurrenceUtc = GetNextOccurrenceForCronExpression(
            cronExpression: options.Value.StageScheduledReleasesFunctionCronSchedule,
            fromUtc: fromUtc,
            toUtc: toUtc,
            timeZone: ukTimeZone
        );

        if (nextOccurrenceUtc.HasValue)
        {
            // Publishing won't occur unless there's an occurrence of (2) after (1) but before the end of the range
            return GetNextOccurrenceForCronExpression(
                cronExpression: options.Value.PublishScheduledReleasesFunctionCronSchedule,
                fromUtc: nextOccurrenceUtc.Value,
                toUtc: toUtc,
                timeZone: ukTimeZone
            ).HasValue;
        }

        return false;
    }

    private static DateTimeOffset? GetNextOccurrenceForCronExpression(
        string cronExpression,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        TimeZoneInfo timeZone,
        bool fromInclusive = true,
        bool toInclusive = true
    )
    {
        // Azure functions use a sixth field at the beginning of cron expressions for time precision in seconds
        // so we need to allow for this when parsing them.
        var expression = CronExpression.Parse(
            cronExpression,
            CronExpressionHasSecondPrecision(cronExpression) ? CronFormat.IncludeSeconds : CronFormat.Standard
        );

        var occurrences = expression.GetOccurrences(fromUtc, toUtc, timeZone, fromInclusive, toInclusive).ToList();
        return occurrences.Any() ? occurrences[0] : null;
    }

    private async Task<Either<ActionResult, Unit>> ValidateReleaseWithChecklist(ReleaseVersion releaseVersion)
    {
        if (releaseVersion.ApprovalStatus != ReleaseApprovalStatus.Approved)
        {
            return Unit.Instance;
        }

        var errors = (await releaseChecklistService.GetErrors(releaseVersion)).Select(error => error.Code).ToList();

        if (!errors.Any())
        {
            return Unit.Instance;
        }

        return ValidationActionResult(errors);
    }
}
