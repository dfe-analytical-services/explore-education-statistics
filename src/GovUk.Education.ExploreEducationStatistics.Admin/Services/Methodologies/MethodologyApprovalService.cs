#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class MethodologyApprovalService : IMethodologyApprovalService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly ContentDbContext _context;
        private readonly IMethodologyContentService _methodologyContentService;
        private readonly IMethodologyFileRepository _methodologyFileRepository;
        private readonly IMethodologyVersionRepository _methodologyVersionRepository;
        private readonly IMethodologyImageService _methodologyImageService;
        private readonly IPublishingService _publishingService;
        private readonly IUserService _userService;
        private readonly IMethodologyCacheService _methodologyCacheService;

        public MethodologyApprovalService(
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            ContentDbContext context,
            IMethodologyContentService methodologyContentService,
            IMethodologyFileRepository methodologyFileRepository,
            IMethodologyVersionRepository methodologyVersionRepository,
            IMethodologyImageService methodologyImageService,
            IPublishingService publishingService,
            IUserService userService, 
            IMethodologyCacheService methodologyCacheService)
        {
            _persistenceHelper = persistenceHelper;
            _context = context;
            _methodologyContentService = methodologyContentService;
            _methodologyFileRepository = methodologyFileRepository;
            _methodologyVersionRepository = methodologyVersionRepository;
            _methodologyImageService = methodologyImageService;
            _publishingService = publishingService;
            _userService = userService;
            _methodologyCacheService = methodologyCacheService;
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
            if (!request.IsStatusUpdateForMethodology(methodologyVersionToUpdate))
            {
                // Status unchanged
                return methodologyVersionToUpdate;
            }

            return await 
                CheckCanUpdateStatus(methodologyVersionToUpdate, request.Status)
                .OnSuccessDo(methodology => CheckMethodologyCanDependOnRelease(methodology, request))
                .OnSuccessDo(RemoveUnusedImages)
                .OnSuccess(async methodology =>
                {
                    methodology.Status = request.Status;
                    methodology.PublishingStrategy = request.PublishingStrategy;
                    methodology.ScheduledWithReleaseId = WithRelease == request.PublishingStrategy
                        ? request.WithReleaseId
                        : null;
                    methodology.InternalReleaseNote = Approved == request.Status
                        ? request.LatestInternalReleaseNote
                        : null;

                    methodology.Updated = DateTime.UtcNow;

                    _context.MethodologyVersions.Update(methodology);

                    var isPubliclyAccessible = await _methodologyVersionRepository.IsPubliclyAccessible(methodology.Id);
                    
                    if (isPubliclyAccessible)
                    {
                        methodology.Published = DateTime.UtcNow;

                        await _publishingService.PublishMethodologyFiles(methodology.Id);
                    }

                    _context.MethodologyVersions.Update(methodology);
                    await _context.SaveChangesAsync();

                    if (isPubliclyAccessible)
                    {
                        // Update the 'All Methodologies' cache item
                        await _methodologyCacheService.UpdateSummariesTree();
                    }

                    return methodology;
                });
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

                    await _context.Entry(methodologyVersion)
                        .Reference(m => m.Methodology)
                        .LoadAsync();

                    await _context.Entry(methodologyVersion.Methodology)
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

        private Task<Either<ActionResult, MethodologyVersion>> CheckCanUpdateStatus(
            MethodologyVersion methodologyVersion,
            MethodologyStatus requestedStatus)
        {
            return requestedStatus switch
            {
                Draft => _userService.CheckCanMarkMethodologyVersionAsDraft(methodologyVersion),
                Approved => _userService.CheckCanApproveMethodologyVersion(methodologyVersion),
                _ => throw new ArgumentOutOfRangeException(nameof(requestedStatus), "Unexpected status")
            };
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
    }
}
