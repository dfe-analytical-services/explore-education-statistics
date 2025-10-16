#nullable enable
using System.Globalization;
using Cronos;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.CronExpressionUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseApprovalService : IReleaseApprovalService
{
    private readonly ContentDbContext _context;
    private readonly DateTimeProvider _dateTimeProvider;
    private readonly IUserService _userService;
    private readonly IPublishingService _publishingService;
    private readonly IReleaseChecklistService _releaseChecklistService;
    private readonly IContentService _contentService;
    private readonly IPreReleaseUserService _preReleaseUserService;
    private readonly IReleaseFileRepository _releaseFileRepository;
    private readonly IReleaseFileService _releaseFileService;
    private readonly ReleaseApprovalOptions _options;
    private readonly IUserReleaseRoleService _userReleaseRoleService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IUserRepository _userRepository;

    public ReleaseApprovalService(
        ContentDbContext context,
        DateTimeProvider dateTimeProvider,
        IUserService userService,
        IPublishingService publishingService,
        IReleaseChecklistService releaseChecklistService,
        IContentService contentService,
        IPreReleaseUserService preReleaseUserService,
        IReleaseFileRepository releaseFileRepository,
        IReleaseFileService releaseFileService,
        IOptions<ReleaseApprovalOptions> options,
        IUserReleaseRoleService userReleaseRoleService,
        IEmailTemplateService emailTemplateService,
        IUserRepository userRepository
    )
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _userService = userService;
        _publishingService = publishingService;
        _releaseChecklistService = releaseChecklistService;
        _contentService = contentService;
        _preReleaseUserService = preReleaseUserService;
        _releaseFileRepository = releaseFileRepository;
        _releaseFileService = releaseFileService;
        _userReleaseRoleService = userReleaseRoleService;
        _emailTemplateService = emailTemplateService;
        _userRepository = userRepository;
        _options = options.Value;
    }

    public async Task<Either<ActionResult, List<ReleaseStatusViewModel>>> ListReleaseStatuses(Guid releaseVersionId)
    {
        return await _context
            .ReleaseVersions.SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId)
            .OnSuccess(_userService.CheckCanViewReleaseVersionStatusHistory)
            .OnSuccess(async releaseVersion =>
            {
                return await _context
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
        return await _context
            .ReleaseVersions.HydrateReleaseForChecklist()
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId)
            .OnSuccessDo(releaseVersion =>
                _userService.CheckCanUpdateReleaseVersionStatus(releaseVersion, request.ApprovalStatus)
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
                        ? _dateTimeProvider.UtcNow
                        : request.PublishScheduledDate;

                var releaseStatus = new ReleaseStatus
                {
                    Id = Guid.NewGuid(),
                    ReleaseVersion = releaseVersion,
                    InternalReleaseNote = request.InternalReleaseNote,
                    ApprovalStatus = request.ApprovalStatus,
                    CreatedById = _userService.GetUserId(),
                };

                var releasePublishingKey = new ReleasePublishingKey(releaseVersionId, releaseStatus.Id);

                return await ValidateReleaseWithChecklist(releaseVersion)
                    .OnSuccess(() =>
                        SendEmailNotificationsAndInvites(request, releaseVersion)
                            .OnSuccess(() => NotifyPublisher(releasePublishingKey, request, oldStatus))
                            .OnSuccessDo(async () =>
                            {
                                _context.ReleaseVersions.Update(releaseVersion);
                                await _context.AddAsync(releaseStatus);
                                await _context.SaveChangesAsync();
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
            return await _publishingService.ReleaseChanged(
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
            return await SendPreReleaseUserInviteEmails(releaseVersion);
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
        var userReleaseRoles = await _userReleaseRoleService.ListUserReleaseRolesByPublication(
            ReleaseRole.Approver,
            releaseVersion.Publication.Id
        );

        var userPublicationRoles = await _context
            .UserPublicationRoles.Include(upr => upr.User)
            .Where(upr => upr.PublicationId == releaseVersion.PublicationId && upr.Role == PublicationRole.Allower)
            .ToListAsync();

        var notifyHigherReviewers = userReleaseRoles.Any() || userPublicationRoles.Any();

        if (notifyHigherReviewers)
        {
            return userReleaseRoles
                .Select(urr => urr.User.Email)
                .Concat(userPublicationRoles.Select(upr => upr.User.Email))
                .Distinct()
                .Select(email => _emailTemplateService.SendReleaseHigherReviewEmail(email, releaseVersion))
                .OnSuccessAllReturnVoid();
        }

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, Unit>> SendPreReleaseUserInviteEmails(ReleaseVersion releaseVersion)
    {
        var unsentUserReleaseInvites = await _context
            .UserReleaseInvites.Where(i =>
                i.ReleaseVersionId == releaseVersion.Id && i.Role == ReleaseRole.PrereleaseViewer && !i.EmailSent
            )
            .ToListAsync();

        var emailResults = await unsentUserReleaseInvites
            .ToAsyncEnumerable()
            .SelectAwait(async invite =>
            {
                var activeUser = await _userRepository.FindActiveUserByEmail(invite.Email);
                return await _preReleaseUserService
                    .SendPreReleaseInviteEmail(releaseVersion.Id, invite.Email.ToLower(), isNewUser: activeUser is null)
                    .OnSuccessDo(() => _preReleaseUserService.MarkInviteEmailAsSent(invite));
            })
            .ToListAsync();

        return emailResults.OnSuccessAllReturnVoid();
    }

    private async Task<Either<ActionResult, Unit>> RemoveUnusedImages(Guid releaseVersionId)
    {
        return await _contentService
            .GetContentBlocks<HtmlBlock>(releaseVersionId)
            .OnSuccess(async contentBlocks =>
            {
                var contentImageIds = contentBlocks
                    .SelectMany(contentBlock => HtmlImageUtil.GetReleaseImages(contentBlock.Body))
                    .Distinct();

                var imageFiles = await _releaseFileRepository.GetByFileType(releaseVersionId, types: FileType.Image);

                var unusedImages = imageFiles
                    .Where(file => !contentImageIds.Contains(file.File.Id))
                    .Select(file => file.File.Id)
                    .ToList();

                if (unusedImages.Any())
                {
                    return await _releaseFileService.Delete(
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
            if (string.IsNullOrEmpty(request.PublishScheduled))
            {
                return ValidationActionResult(PublishDateCannotBeEmpty);
            }

            var publishDate = DateTime.ParseExact(
                request.PublishScheduled,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None
            );

            // Check if publishing will occur on the publish date as there may be no scheduled occurrences
            // of the two Azure Functions which perform publishing.
            if (!CheckPublishDateCanBeScheduled(publishDate))
            {
                return ValidationActionResult(PublishDateCannotBeScheduled);
            }
        }

        return Unit.Instance;
    }

    private bool CheckPublishDateCanBeScheduled(DateTime publishDate)
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

        var ukTimeZone = DateTimeExtensions.GetUkTimeZone();
        var fromUtc = TimeZoneInfo.ConvertTimeToUtc(publishDate.Date, ukTimeZone);
        var toUtc = fromUtc.AddDays(1).AddTicks(-1);

        // The publish date cannot be scheduled if it's already passed
        if (_dateTimeProvider.UtcNow > toUtc)
        {
            return false;
        }

        // The range should begin now rather than at midnight if the publish date is today
        if (_dateTimeProvider.UtcNow > fromUtc)
        {
            fromUtc = _dateTimeProvider.UtcNow;
        }

        // Publishing won't occur unless there's an occurrence of (1) between the publishing range
        var nextOccurrenceUtc = GetNextOccurrenceForCronExpression(
            cronExpression: _options.StageScheduledReleasesFunctionCronSchedule,
            fromUtc: fromUtc,
            toUtc: toUtc,
            timeZone: ukTimeZone
        );

        if (nextOccurrenceUtc.HasValue)
        {
            // Publishing won't occur unless there's an occurrence of (2) after (1) but before the end of the range
            return GetNextOccurrenceForCronExpression(
                cronExpression: _options.PublishScheduledReleasesFunctionCronSchedule,
                fromUtc: nextOccurrenceUtc.Value,
                toUtc: toUtc,
                timeZone: ukTimeZone
            ).HasValue;
        }

        return false;
    }

    private static DateTime? GetNextOccurrenceForCronExpression(
        string cronExpression,
        DateTime fromUtc,
        DateTime toUtc,
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

        var errors = (await _releaseChecklistService.GetErrors(releaseVersion)).Select(error => error.Code).ToList();

        if (!errors.Any())
        {
            return Unit.Instance;
        }

        return ValidationActionResult(errors);
    }
}
