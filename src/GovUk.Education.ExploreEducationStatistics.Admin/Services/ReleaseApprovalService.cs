#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseApprovalService : IReleaseApprovalService
    {
        private readonly ContentDbContext _context;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IPublishingService _publishingService;
        private readonly IReleaseChecklistService _releaseChecklistService;
        private readonly IContentService _contentService;
        private readonly IPreReleaseUserService _preReleaseUserService;
        private readonly IReleaseFileRepository _releaseFileRepository;
        private readonly IReleaseFileService _releaseFileService;

        public ReleaseApprovalService(
            ContentDbContext context,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            IPublishingService publishingService,
            IReleaseChecklistService releaseChecklistService,
            IContentService contentService,
            IPreReleaseUserService preReleaseUserService,
            IReleaseFileRepository releaseFileRepository,
            IReleaseFileService releaseFileService)
        {
            _context = context;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _publishingService = publishingService;
            _releaseChecklistService = releaseChecklistService;
            _contentService = contentService;
            _preReleaseUserService = preReleaseUserService;
            _releaseFileRepository = releaseFileRepository;
            _releaseFileService = releaseFileService;
        }

        public async Task<Either<ActionResult, List<ReleaseStatusViewModel>>> GetReleaseStatuses(
            Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId, release =>
                    release.Include(r => r.ReleaseStatuses)
                        .ThenInclude(rs => rs.CreatedBy))
                .OnSuccess(_userService.CheckCanViewReleaseStatusHistory)
                .OnSuccess(release =>
                {
                    var allReleaseVersionIds = _context.Releases
                        .Where(r => r.PublicationId == release.PublicationId
                                    && r.ReleaseName == release.ReleaseName
                                    && r.TimePeriodCoverage == release.TimePeriodCoverage)
                        .Select(r => r.Id)
                        .ToList();
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
            Guid releaseId, ReleaseStatusCreateViewModel request)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId, ReleaseChecklistService.HydrateReleaseForChecklist)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccessDo(release => _userService.CheckCanUpdateReleaseStatus(release, request.ApprovalStatus))
                .OnSuccess(async release =>
                {
                    if (request.ApprovalStatus != ReleaseApprovalStatus.Approved && release.Published.HasValue)
                    {
                        return ValidationActionResult(PublishedReleaseCannotBeUnapproved);
                    }

                    if (request.ApprovalStatus == ReleaseApprovalStatus.Approved
                        && request.PublishMethod == PublishMethod.Scheduled
                        && !request.PublishScheduledDate.HasValue)
                    {
                        return ValidationActionResult(ApprovedReleaseMustHavePublishScheduledDate);
                    }

                    var oldStatus = release.ApprovalStatus;

                    release.ApprovalStatus = request.ApprovalStatus;
                    release.NextReleaseDate = request.NextReleaseDate;
                    release.PublishScheduled = request.PublishMethod == PublishMethod.Immediate &&
                                               request.ApprovalStatus == ReleaseApprovalStatus.Approved
                        ? DateTime.UtcNow
                        : request.PublishScheduledDate;

                    // NOTE: Subscribers should be notified if the release is approved and isn't amended,
                    //       OR if the release is an amendment, is approved, and NotifySubscribers is true
                    var notifySubscribers = request.ApprovalStatus == ReleaseApprovalStatus.Approved &&
                        (!release.Amendment || request.NotifySubscribers.HasValue && request.NotifySubscribers.Value);

                    var releaseStatus = new ReleaseStatus
                    {
                        Release = release,
                        InternalReleaseNote = request.LatestInternalReleaseNote,
                        NotifySubscribers = notifySubscribers,
                        ApprovalStatus = request.ApprovalStatus,
                        Created = DateTime.UtcNow,
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

                            _context.Update(release);
                            await _context.SaveChangesAsync();

                            if (request.ApprovalStatus == ReleaseApprovalStatus.Approved)
                            {
                                await _preReleaseUserService.SendPreReleaseUserInviteEmails(release);
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
}
