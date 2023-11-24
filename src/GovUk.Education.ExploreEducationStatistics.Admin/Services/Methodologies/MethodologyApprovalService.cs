#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class MethodologyApprovalService : IMethodologyApprovalService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly ContentDbContext _contentDbContext;
        private readonly IMethodologyContentService _methodologyContentService;
        private readonly IMethodologyFileRepository _methodologyFileRepository;
        private readonly IMethodologyImageService _methodologyImageService;
        private readonly IPublishingService _publishingService;
        private readonly IUserService _userService;
        private readonly IUserReleaseRoleService _userReleaseRoleService;
        private readonly IMethodologyCacheService _methodologyCacheService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IRedirectsCacheService _redirectsCacheService;

        public MethodologyApprovalService(
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            ContentDbContext contentDbContext,
            IMethodologyContentService methodologyContentService,
            IMethodologyFileRepository methodologyFileRepository,
            IMethodologyImageService methodologyImageService,
            IPublishingService publishingService,
            IUserService userService,
            IUserReleaseRoleService userReleaseRoleService,
            IMethodologyCacheService methodologyCacheService,
            IEmailTemplateService emailTemplateService,
            IRedirectsCacheService redirectsCacheService)
        {
            _persistenceHelper = persistenceHelper;
            _contentDbContext = contentDbContext;
            _methodologyContentService = methodologyContentService;
            _methodologyFileRepository = methodologyFileRepository;
            _methodologyImageService = methodologyImageService;
            _publishingService = publishingService;
            _userService = userService;
            _userReleaseRoleService = userReleaseRoleService;
            _methodologyCacheService = methodologyCacheService;
            _emailTemplateService = emailTemplateService;
            _redirectsCacheService = redirectsCacheService;
        }

        public async Task<Either<ActionResult, MethodologyVersion>> UpdateApprovalStatus(
            Guid methodologyVersionId,
            MethodologyApprovalUpdateRequest request)
        {
            return await _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(methodologyVersionId, q =>
                    q.Include(m => m.Methodology))
                .OnSuccess(methodology => UpdateStatus(methodology, request));
        }

        private async Task<Either<ActionResult, MethodologyVersion>> UpdateStatus(
            MethodologyVersion methodologyVersionToUpdate,
            MethodologyApprovalUpdateRequest request)
        {
            if (!request.IsStatusUpdateRequired(methodologyVersionToUpdate))
            {
                return methodologyVersionToUpdate;
            }

            return await 
                _userService.CheckCanUpdateMethodologyVersionStatus(methodologyVersionToUpdate, request.Status)
                    .OnSuccessDo(methodologyVersion => CheckMethodologyCanDependOnRelease(methodologyVersion, request))
                    .OnSuccessDo(RemoveUnusedImages)
                    .OnSuccess(async methodologyVersion =>
                    {
                        _contentDbContext.MethodologyVersions.Update(methodologyVersion);

                        methodologyVersion.Status = request.Status;
                        methodologyVersion.PublishingStrategy = request.PublishingStrategy;
                        methodologyVersion.ScheduledWithReleaseId = WithRelease == request.PublishingStrategy
                            ? request.WithReleaseId
                            : null;

                        methodologyVersion.Updated = DateTime.UtcNow;

                        // We cannot rely on Methodology.LatestPublishedVersionId, as it may now be incorrect,
                        // if we are approving and publishing this methodology version.
                        var isToBePublished = await IsToBePublished(methodologyVersion);

                        if (isToBePublished)
                        {
                            methodologyVersion.Published = DateTime.UtcNow;
                            methodologyVersion.Methodology.LatestPublishedVersionId = methodologyVersion.Id;

                            await _publishingService.PublishMethodologyFiles(methodologyVersion.Id);
                        }

                        var methodologyStatus = new MethodologyStatus
                        {
                            MethodologyVersionId = methodologyVersion.Id,
                            InternalReleaseNote = request.LatestInternalReleaseNote,
                            ApprovalStatus = request.Status,
                            CreatedById = _userService.GetUserId(),
                        };
                        await _contentDbContext.MethodologyStatus.AddAsync(methodologyStatus);

                        await _contentDbContext.SaveChangesAsync();

                        if (isToBePublished)
                        {
                            await _methodologyCacheService.UpdateSummariesTree();
                            await _redirectsCacheService.UpdateRedirects();
                        }

                        if (request.Status == MethodologyApprovalStatus.HigherLevelReview)
                        {
                            await NotifyApprovers(methodologyVersion);
                        }

                        return methodologyVersion;
                    });
        }

        private async Task NotifyApprovers(MethodologyVersion methodologyVersion)
        {
            var owningPublicationId = await _contentDbContext.PublicationMethodologies
                .Where(pm => pm.MethodologyId == methodologyVersion.MethodologyId
                             && pm.Owner)
                .Select(pm => pm.PublicationId)
                .SingleAsync();

            var userReleaseRoles = await _userReleaseRoleService
                .ListUserReleaseRolesByPublication(ReleaseRole.Approver, owningPublicationId);

            var userPublicationRoles = await _contentDbContext.UserPublicationRoles
                .Include(upr => upr.User)
                .Where(upr => owningPublicationId == upr.PublicationId
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
                        _emailTemplateService.SendMethodologyHigherReviewEmail(email, methodologyVersion.Id, methodologyVersion.Title);
                    });
            }
        }

        private async Task<Either<ActionResult, Unit>> CheckMethodologyCanDependOnRelease(
            MethodologyVersion methodologyVersion,
            MethodologyApprovalUpdateRequest request)
        {
            if (request.PublishingStrategy != WithRelease)
            {
                return Unit.Instance;
            }

            if (!request.WithReleaseId.HasValue)
            {
                return new NotFoundResult();
            }

            // Check that this release exists, that it's not already published, and that it's using the methodology
            return await _persistenceHelper
                .CheckEntityExists<Release>(request.WithReleaseId.Value)
                .OnSuccess<ActionResult, Release, Unit>(async release =>
                {
                    if (release.Live)
                    {
                        return ValidationActionResult(MethodologyCannotDependOnPublishedRelease);
                    }

                    await _contentDbContext.Entry(methodologyVersion)
                        .Reference(m => m.Methodology)
                        .LoadAsync();

                    await _contentDbContext.Entry(methodologyVersion.Methodology)
                        .Collection(mp => mp.Publications)
                        .LoadAsync();

                    var publicationIds = methodologyVersion.Methodology.Publications
                        .Select(pm => pm.PublicationId)
                        .ToList();

                    if (!publicationIds.Contains(release.PublicationId))
                    {
                        return ValidationActionResult(MethodologyCannotDependOnRelease);
                    }

                    return Unit.Instance;
                });
        }

        private async Task<Either<ActionResult, Unit>> RemoveUnusedImages(MethodologyVersion methodologyVersion)
        {
            return await _methodologyContentService.GetContentBlocks<HtmlBlock>(methodologyVersion.Id)
                .OnSuccess(async contentBlocks =>
                {
                    var contentImageIds = contentBlocks.SelectMany(contentBlock =>
                            HtmlImageUtil.GetMethodologyImages(contentBlock.Body))
                        .Distinct();

                    var imageFiles = await _methodologyFileRepository.GetByFileType(methodologyVersion.Id, Image);

                    var unusedImages = imageFiles
                        .Where(file => !contentImageIds.Contains(file.File.Id))
                        .Select(file => file.File.Id)
                        .ToList();

                    if (unusedImages.Any())
                    {
                        return await _methodologyImageService.Delete(methodologyVersion.Id, unusedImages);
                    }

                    return Unit.Instance;
                });
        }
        
        private async Task<bool> IsToBePublished(MethodologyVersion methodologyVersion)
        {
            // A version that's not approved can't be publicly accessible
            if (!methodologyVersion.Approved)
            {
                return false;
            }

            // A methodology version with a newer published version cannot be live
            var nextVersion = await GetNextVersion(methodologyVersion);
            if (nextVersion?.Approved == true)
            {
                // If the next version is scheduled for immediate publishing, we don't need to check if the
                // associated publication is published: the previous version won't be published in either case.
                if (nextVersion.ScheduledForPublishingImmediately ||
                    await IsVersionScheduledForPublishingWithPublishedRelease(nextVersion))
                {
                    return false;
                }
            }

            // A version scheduled for publishing immediately is restricted from public view until it's used by
            // a publication that's published
            if (methodologyVersion.ScheduledForPublishingImmediately)
            {
                return await PublicationsHaveAtLeastOnePublishedRelease(methodologyVersion);
            }

            // A version scheduled for publishing with a release is only publicly accessible if that release is published
            return await IsVersionScheduledForPublishingWithPublishedRelease(methodologyVersion);
        }

        private async Task<bool> PublicationsHaveAtLeastOnePublishedRelease(MethodologyVersion methodologyVersion)
        {
            await _contentDbContext.Entry(methodologyVersion)
                .Reference(m => m.Methodology)
                .LoadAsync();

            await _contentDbContext.Entry(methodologyVersion.Methodology)
                .Collection(mp => mp.Publications)
                .LoadAsync();

            return methodologyVersion.Methodology.Publications.Any(publicationMethodology =>
            {
                _contentDbContext.Entry(publicationMethodology)
                    .Reference(pm => pm.Publication)
                    .Load();

                return publicationMethodology.Publication.Live;
            });
        }

        private async Task<bool> IsVersionScheduledForPublishingWithPublishedRelease(
            MethodologyVersion methodologyVersion)
        {
            if (!methodologyVersion.ScheduledForPublishingWithRelease)
            {
                return false;
            }

            await _contentDbContext.Entry(methodologyVersion)
                .Reference(m => m.ScheduledWithRelease)
                .LoadAsync();
            return methodologyVersion.ScheduledForPublishingWithPublishedRelease;
        }

        private async Task<MethodologyVersion?> GetNextVersion(MethodologyVersion methodologyVersion)
        {
            await _contentDbContext.Entry(methodologyVersion)
                .Reference(m => m.Methodology)
                .LoadAsync();

            await _contentDbContext.Entry(methodologyVersion.Methodology)
                .Collection(mp => mp.Versions)
                .LoadAsync();

            return methodologyVersion.Methodology.Versions.SingleOrDefault(mv =>
                mv.PreviousVersionId == methodologyVersion.Id);
        }
    }
}
