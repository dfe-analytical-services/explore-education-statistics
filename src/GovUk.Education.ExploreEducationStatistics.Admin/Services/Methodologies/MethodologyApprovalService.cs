#nullable enable
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
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;

public class MethodologyApprovalService(
    IPersistenceHelper<ContentDbContext> persistenceHelper,
    ContentDbContext context,
    IMethodologyContentService methodologyContentService,
    IMethodologyFileRepository methodologyFileRepository,
    IMethodologyVersionRepository methodologyVersionRepository,
    IMethodologyImageService methodologyImageService,
    IPublishingService publishingService,
    IUserService userService,
    IUserReleaseRoleService userReleaseRoleService,
    IUserPublicationRoleRepository userPublicationRoleRepository,
    IMethodologyCacheService methodologyCacheService,
    IEmailTemplateService emailTemplateService,
    IRedirectsCacheService redirectsCacheService
) : IMethodologyApprovalService
{
    public async Task<Either<ActionResult, MethodologyVersion>> UpdateApprovalStatus(
        Guid methodologyVersionId,
        MethodologyApprovalUpdateRequest request
    )
    {
        return await persistenceHelper
            .CheckEntityExists<MethodologyVersion>(methodologyVersionId, q => q.Include(m => m.Methodology))
            .OnSuccess(methodology => UpdateStatus(methodology, request));
    }

    private async Task<Either<ActionResult, MethodologyVersion>> UpdateStatus(
        MethodologyVersion methodologyVersionToUpdate,
        MethodologyApprovalUpdateRequest request
    )
    {
        if (!request.IsStatusUpdateRequired(methodologyVersionToUpdate))
        {
            return methodologyVersionToUpdate;
        }

        return await userService
            .CheckCanUpdateMethodologyVersionStatus(methodologyVersionToUpdate, request.Status)
            .OnSuccessDo(methodologyVersion => CheckMethodologyCanDependOnRelease(methodologyVersion, request))
            .OnSuccessDo(RemoveUnusedImages)
            .OnSuccess(async methodologyVersion =>
            {
                context.MethodologyVersions.Update(methodologyVersion);

                methodologyVersion.Status = request.Status;
                methodologyVersion.PublishingStrategy = request.PublishingStrategy;
                methodologyVersion.ScheduledWithReleaseVersionId =
                    WithRelease == request.PublishingStrategy ? request.WithReleaseId : null;

                methodologyVersion.Updated = DateTime.UtcNow;

                // We cannot rely on Methodology.LatestPublishedVersionId, as it may now be incorrect,
                // if we are approving and publishing this methodology version.
                var isToBePublished = await methodologyVersionRepository.IsToBePublished(methodologyVersion);

                if (isToBePublished)
                {
                    methodologyVersion.Published = DateTime.UtcNow;
                    methodologyVersion.Methodology.LatestPublishedVersionId = methodologyVersion.Id;

                    await publishingService.PublishMethodologyFiles(methodologyVersion.Id);
                }

                var methodologyStatus = new MethodologyStatus
                {
                    MethodologyVersionId = methodologyVersion.Id,
                    InternalReleaseNote = request.LatestInternalReleaseNote,
                    ApprovalStatus = request.Status,
                    CreatedById = userService.GetUserId(),
                };
                await context.MethodologyStatus.AddAsync(methodologyStatus);

                await context.SaveChangesAsync();

                if (isToBePublished)
                {
                    await methodologyCacheService.UpdateSummariesTree();

                    await context.Entry(methodologyVersion).Reference(mv => mv.Methodology).LoadAsync();

                    var redundantRedirects = await context
                        .MethodologyRedirects.Where(mr => mr.Slug == methodologyVersion.Slug)
                        .ToListAsync();
                    context.MethodologyRedirects.RemoveRange(redundantRedirects);
                    await context.SaveChangesAsync();

                    await redirectsCacheService.UpdateRedirects();
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
        var owningPublicationId = await context
            .PublicationMethodologies.Where(pm => pm.MethodologyId == methodologyVersion.MethodologyId && pm.Owner)
            .Select(pm => pm.PublicationId)
            .SingleAsync();

        var userReleaseRoles = await userReleaseRoleService.ListLatestActiveUserReleaseRolesByPublication(
            publicationId: owningPublicationId,
            rolesToInclude: ReleaseRole.Approver
        );

        var userPublicationRoles = await userPublicationRoleRepository
            .Query()
            .WhereForPublication(owningPublicationId)
            .WhereRolesIn(PublicationRole.Allower)
            .Include(upr => upr.User)
            .ToListAsync();

        var notifyHigherReviewers = userReleaseRoles.Any() || userPublicationRoles.Any();
        if (notifyHigherReviewers)
        {
            userReleaseRoles
                .Select(urr => urr.User.Email)
                .Concat(userPublicationRoles.Select(upr => upr.User.Email))
                .Distinct()
                .ForEach(email =>
                {
                    emailTemplateService.SendMethodologyHigherReviewEmail(
                        email,
                        methodologyVersion.Id,
                        methodologyVersion.Title
                    );
                });
        }
    }

    private async Task<Either<ActionResult, Unit>> CheckMethodologyCanDependOnRelease(
        MethodologyVersion methodologyVersion,
        MethodologyApprovalUpdateRequest request
    )
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
        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(request.WithReleaseId.Value)
            .OnSuccess<ActionResult, ReleaseVersion, Unit>(async release =>
            {
                if (release.Live)
                {
                    return ValidationActionResult(MethodologyCannotDependOnPublishedRelease);
                }

                await context.Entry(methodologyVersion).Reference(m => m.Methodology).LoadAsync();

                await context.Entry(methodologyVersion.Methodology).Collection(mp => mp.Publications).LoadAsync();

                var publicationIds = methodologyVersion
                    .Methodology.Publications.Select(pm => pm.PublicationId)
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
        return await methodologyContentService
            .GetContentBlocks<HtmlBlock>(methodologyVersion.Id)
            .OnSuccess(async contentBlocks =>
            {
                var contentImageIds = contentBlocks
                    .SelectMany(contentBlock => HtmlImageUtil.GetMethodologyImages(contentBlock.Body))
                    .Distinct();

                var imageFiles = await methodologyFileRepository.GetByFileType(methodologyVersion.Id, Image);

                var unusedImages = imageFiles
                    .Where(file => !contentImageIds.Contains(file.File.Id))
                    .Select(file => file.File.Id)
                    .ToList();

                if (unusedImages.Any())
                {
                    return await methodologyImageService.Delete(
                        methodologyVersionId: methodologyVersion.Id,
                        fileIds: unusedImages,
                        forceDelete: true
                    );
                }

                return Unit.Instance;
            });
    }
}
