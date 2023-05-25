#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Cronos;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.CronExpressionUtil;
using IReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseApprovalService : IReleaseApprovalService
    {
        private readonly ContentDbContext _context;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly DateTimeProvider _dateTimeProvider;
        private readonly IUserService _userService;
        private readonly IPublishingService _publishingService;
        private readonly IReleaseChecklistService _releaseChecklistService;
        private readonly IContentService _contentService;
        private readonly IPreReleaseUserService _preReleaseUserService;
        private readonly IReleaseFileRepository _releaseFileRepository;
        private readonly IReleaseFileService _releaseFileService;
        private readonly IReleaseRepository _releaseRepository;
        private readonly ReleaseApprovalOptions _options;
        private readonly IUserReleaseRoleService _userReleaseRoleService;
        private readonly IEmailTemplateService _emailTemplateService;

        public ReleaseApprovalService(
            ContentDbContext context,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            DateTimeProvider dateTimeProvider,
            IUserService userService,
            IPublishingService publishingService,
            IReleaseChecklistService releaseChecklistService,
            IContentService contentService,
            IPreReleaseUserService preReleaseUserService,
            IReleaseFileRepository releaseFileRepository,
            IReleaseFileService releaseFileService,
            IReleaseRepository releaseRepository,
            IOptions<ReleaseApprovalOptions> options,
            IUserReleaseRoleService userReleaseRoleService,
            IEmailTemplateService emailTemplateService
        )
        {
            _context = context;
            _persistenceHelper = persistenceHelper;
            _dateTimeProvider = dateTimeProvider;
            _userService = userService;
            _publishingService = publishingService;
            _releaseChecklistService = releaseChecklistService;
            _contentService = contentService;
            _preReleaseUserService = preReleaseUserService;
            _releaseFileRepository = releaseFileRepository;
            _releaseFileService = releaseFileService;
            _releaseRepository = releaseRepository;
            _userReleaseRoleService = userReleaseRoleService;
            _emailTemplateService = emailTemplateService;
            _options = options.Value;
        }

        public async Task<Either<ActionResult, List<ReleaseStatusViewModel>>> GetReleaseStatuses(
            Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId, release =>
                    release.Include(r => r.ReleaseStatuses)
                        .ThenInclude(rs => rs.CreatedBy))
                .OnSuccess(_userService.CheckCanViewReleaseStatusHistory)
                .OnSuccess(async release =>
                {
                    var allReleaseVersionIds = await _releaseRepository.GetAllReleaseVersionIds(release);
                    return _context.ReleaseStatus
                        .Include(rs => rs.Release)
                        .Include(rs => rs.CreatedBy)
                        .Where(rs => allReleaseVersionIds.Contains(rs.ReleaseId))
                        .ToList()
                        .Select(rs =>
                            new ReleaseStatusViewModel
                            {
                                ReleaseStatusId = rs.Id,
                                InternalReleaseNote = rs.InternalReleaseNote,
                                ApprovalStatus = rs.ApprovalStatus,
                                Created = rs.Created,
                                CreatedByEmail = rs.CreatedBy?.Email,
                                ReleaseVersion = rs.Release.Version,
                            })
                        .OrderByDescending(vm => vm.Created)
                        .ToList();
                });
        }

        public async Task<Either<ActionResult, Unit>> CreateReleaseStatus(
            Guid releaseId,
            ReleaseStatusCreateRequest request)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId, ReleaseChecklistService.HydrateReleaseForChecklist)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccessDo(release => _userService.CheckCanUpdateReleaseStatus(release, request.ApprovalStatus))
                .OnSuccessDo(() => ValidatePublishDate(request))
                .OnSuccess(async release =>
                {
                    if (request.ApprovalStatus != ReleaseApprovalStatus.Approved && release.Published.HasValue)
                    {
                        return ValidationActionResult(PublishedReleaseCannotBeUnapproved);
                    }

                    var oldStatus = release.ApprovalStatus;

                    release.ApprovalStatus = request.ApprovalStatus;
                    release.NextReleaseDate = request.NextReleaseDate;
                    release.NotifySubscribers = release.Version == 0 || (request.NotifySubscribers ?? false);
                    release.UpdatePublishedDate = request.UpdatePublishedDate ?? false;
                    release.PublishScheduled = request.PublishMethod == PublishMethod.Immediate
                        ? _dateTimeProvider.UtcNow
                        : request.PublishScheduledDate;

                    var releaseStatus = new ReleaseStatus
                    {
                        Release = release,
                        InternalReleaseNote = request.InternalReleaseNote,
                        ApprovalStatus = request.ApprovalStatus,
                        CreatedById = _userService.GetUserId()
                    };

                    return await ValidateReleaseWithChecklist(release)
                        .OnSuccessDo(() => RemoveUnusedImages(release.Id))
                        .OnSuccessVoid(async () =>
                        {
                            _context.Releases.Update(release);
                            await _context.AddAsync(releaseStatus);
                            await _context.SaveChangesAsync();

                            // Only need to inform Publisher if changing release approval status to or from Approved
                            if (oldStatus == ReleaseApprovalStatus.Approved ||
                                request.ApprovalStatus == ReleaseApprovalStatus.Approved)
                            {
                                await _publishingService.ReleaseChanged(
                                    releaseId,
                                    releaseStatus.Id,
                                    request.PublishMethod == PublishMethod.Immediate
                                );
                            }

                            switch (request.ApprovalStatus)
                            {
                                case ReleaseApprovalStatus.Approved:
                                    if (request.PublishMethod == PublishMethod.Scheduled)
                                    {
                                        await _preReleaseUserService.SendPreReleaseUserInviteEmails(release.Id);
                                    }
                                    break;

                                case ReleaseApprovalStatus.HigherLevelReview:
                                    var userReleaseRoles =
                                        await _userReleaseRoleService.ListUserReleaseRolesByPublication(
                                            ReleaseRole.Approver,
                                            release.Publication.Id);

                                    var userPublicationRoles = await _context.UserPublicationRoles
                                        .Include(upr => upr.User)
                                        .Where(upr => upr.PublicationId == release.PublicationId
                                                      && upr.Role == PublicationRole.Approver)
                                        .ToListAsync();

                                    var notifyHigherReviewers = userReleaseRoles.Any() || userPublicationRoles.Any();
                                    if (notifyHigherReviewers)
                                    {
                                        userReleaseRoles.Select(urr => urr.User.Email)
                                            .Concat(userPublicationRoles.Select(upr => upr.User.Email))
                                            .Distinct()
                                            .ForEach(email =>
                                            {
                                                _emailTemplateService.SendHigherReviewEmail(email, release);
                                            });
                                    }
                                    break;
                            }
                        });
                });
        }

        private async Task<Either<ActionResult, Unit>> RemoveUnusedImages(Guid releaseId)
        {
            return await _contentService.GetContentBlocks<HtmlBlock>(releaseId)
                .OnSuccess(async contentBlocks =>
                {
                    var contentImageIds = contentBlocks.SelectMany(contentBlock =>
                            HtmlImageUtil.GetReleaseImages(contentBlock.Body))
                        .Distinct();

                    var imageFiles = await _releaseFileRepository.GetByFileType(releaseId, FileType.Image);

                    var unusedImages = imageFiles
                        .Where(file => !contentImageIds.Contains(file.File.Id))
                        .Select(file => file.File.Id)
                        .ToList();

                    if (unusedImages.Any())
                    {
                        return await _releaseFileService.Delete(releaseId, unusedImages);
                    }

                    return Unit.Instance;
                });
        }

        private Either<ActionResult, Unit> ValidatePublishDate(ReleaseStatusCreateRequest request)
        {
            if (request.ApprovalStatus == ReleaseApprovalStatus.Approved
                && request.PublishMethod == PublishMethod.Scheduled)
            {
                // Publish date must be set
                if (string.IsNullOrEmpty(request.PublishScheduled))
                {
                    return ValidationActionResult(PublishDateCannotBeEmpty);
                }

                var publishDate = DateTime.ParseExact(request.PublishScheduled,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None);

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
            // PublishReleases (1) - Runs tasks for the releases that are scheduled to be published.
            // PublishStagedReleaseContent (2) - Runs after (1) and completes publishing of releases.

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
                cronExpression: _options.PublishReleasesCronSchedule,
                fromUtc: fromUtc,
                toUtc: toUtc,
                timeZoneInfo: ukTimeZone);

            if (nextOccurrenceUtc.HasValue)
            {
                // Publishing won't occur unless there's an occurrence of (2) after (1) but before the end of the range
                return GetNextOccurrenceForCronExpression(
                    cronExpression: _options.PublishReleaseContentCronSchedule,
                    fromUtc: nextOccurrenceUtc.Value,
                    toUtc: toUtc,
                    timeZoneInfo: ukTimeZone).HasValue;
            }

            return false;
        }

        private static DateTime? GetNextOccurrenceForCronExpression(string cronExpression,
            DateTime fromUtc,
            DateTime toUtc,
            TimeZoneInfo timeZoneInfo,
            bool fromInclusive = true,
            bool toInclusive = true)
        {
            // Azure functions use a sixth field at the beginning of cron expressions for time precision in seconds
            // so we need to allow for this when parsing them.
            var expression = CronExpression.Parse(cronExpression,
                CronExpressionHasSecondPrecision(cronExpression) ? CronFormat.IncludeSeconds : CronFormat.Standard);

            var occurrences = expression.GetOccurrences(fromUtc, toUtc, timeZoneInfo, fromInclusive, toInclusive)
                .ToList();
            return occurrences.Any() ? occurrences[0] : null;
        }

        private async Task<Either<ActionResult, Unit>> ValidateReleaseWithChecklist(Release release)
        {
            if (release.ApprovalStatus != ReleaseApprovalStatus.Approved)
            {
                return Unit.Instance;
            }

            var errors = (await _releaseChecklistService.GetErrors(release))
                .Select(error => error.Code)
                .ToList();

            if (!errors.Any())
            {
                return Unit.Instance;
            }

            return ValidationActionResult(errors);
        }
    }

    public record ReleaseApprovalOptions
    {
        public const string ReleaseApproval = "ReleaseApproval";

        public string PublishReleasesCronSchedule { get; init; } = string.Empty;
        public string PublishReleaseContentCronSchedule { get; init; } = string.Empty;
    }
}
