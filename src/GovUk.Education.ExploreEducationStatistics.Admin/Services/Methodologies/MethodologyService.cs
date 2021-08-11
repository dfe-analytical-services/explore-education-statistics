#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class MethodologyService : IMethodologyService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
        private readonly IBlobCacheService _blobCacheService;
        private readonly IMethodologyContentService _methodologyContentService;
        private readonly IMethodologyFileRepository _methodologyFileRepository;
        private readonly IMethodologyRepository _methodologyRepository;
        private readonly IMethodologyImageService _methodologyImageService;
        private readonly IPublishingService _publishingService;
        private readonly IUserService _userService;

        public MethodologyService(
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            ContentDbContext context,
            IMapper mapper,
            IBlobCacheService blobCacheService,
            IMethodologyContentService methodologyContentService,
            IMethodologyFileRepository methodologyFileRepository,
            IMethodologyRepository methodologyRepository,
            IMethodologyImageService methodologyImageService,
            IPublishingService publishingService,
            IUserService userService)
        {
            _persistenceHelper = persistenceHelper;
            _context = context;
            _mapper = mapper;
            _blobCacheService = blobCacheService;
            _methodologyContentService = methodologyContentService;
            _methodologyFileRepository = methodologyFileRepository;
            _methodologyRepository = methodologyRepository;
            _methodologyImageService = methodologyImageService;
            _publishingService = publishingService;
            _userService = userService;
        }

        public Task<Either<ActionResult, MethodologySummaryViewModel>> CreateMethodology(Guid publicationId)
        {
            return _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccess(_userService.CheckCanCreateMethodologyForPublication)
                .OnSuccess(() => _methodologyRepository
                    .CreateMethodologyForPublication(publicationId, _userService.GetUserId())
                )
                .OnSuccess(_mapper.Map<MethodologySummaryViewModel>);
        }

        public async Task<Either<ActionResult, MethodologySummaryViewModel>> GetSummary(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Methodology>(id, HydrateMethodologyForMethodologySummaryViewModel)
                .OnSuccess(_userService.CheckCanViewMethodology)
                .OnSuccess(async methodology =>
                {
                    var publicationLinks = methodology.MethodologyParent.Publications;
                    var owningPublication = BuildPublicationViewModel(publicationLinks.Single(pm => pm.Owner));
                    var otherPublications = publicationLinks.Where(pm => !pm.Owner)
                        .Select(BuildPublicationViewModel)
                        .OrderBy(model => model.Title)
                        .ToList();

                    var viewModel = _mapper.Map<MethodologySummaryViewModel>(methodology);

                    viewModel.OwningPublication = owningPublication;
                    viewModel.OtherPublications = otherPublications;

                    if (methodology.ScheduledForPublishingWithRelease)
                    {
                        await _context.Entry(methodology)
                            .Reference(m => m.ScheduledWithRelease)
                            .LoadAsync();

                        await _context.Entry(methodology.ScheduledWithRelease)
                            .Reference(r => r!.Publication)
                            .LoadAsync();

                        if (methodology.ScheduledWithRelease != null)
                        {
                            var title =
                                $"{methodology.ScheduledWithRelease.Publication.Title} - {methodology.ScheduledWithRelease.Title}";
                            viewModel.ScheduledWithRelease = new TitleAndIdViewModel(
                                methodology.ScheduledWithRelease.Id,
                                title);
                        }
                    }

                    return viewModel;
                });
        }

        public async Task<Either<ActionResult, List<TitleAndIdViewModel>>> GetUnpublishedReleasesUsingMethodology(
            Guid id)
        {
            return await _persistenceHelper.CheckEntityExists<Methodology>(id, queryable =>
                    queryable.Include(m => m.MethodologyParent)
                        .ThenInclude(mp => mp.Publications))
                .OnSuccess(methodology => _userService.CheckCanApproveMethodology(methodology)
                    .OrElse(() => _userService.CheckCanMarkMethodologyAsDraft(methodology)))
                .OnSuccess(async methodology =>
                {
                    // Get all Publications using the Methodology including adopting Publications
                    var publicationIds =
                        methodology.MethodologyParent.Publications.Select(pm => pm.PublicationId);

                    // Get the Releases of those publications
                    var releases = await _context.Releases
                        .Include(r => r.Publication)
                        .Where(r => publicationIds.Contains(r.PublicationId))
                        .ToListAsync();

                    // Return an ordered list of the Releases that are not published
                    return releases.Where(r => !r.Live)
                        .OrderBy(r => r.Publication.Title)
                        .ThenBy(r => r.Year)
                        .ThenBy(r => r.TimePeriodCoverage)
                        .Select(r => new TitleAndIdViewModel(r.Id, $"{r.Publication.Title} - {r.Title}"))
                        .ToList();
                });
        }

        public async Task<Either<ActionResult, MethodologySummaryViewModel>> UpdateMethodology(Guid id,
            MethodologyUpdateRequest request)
        {
            return await _persistenceHelper
                .CheckEntityExists<Methodology>(id, HydrateMethodologyForManageMethodologySummaryViewModel)
                .OnSuccess(methodology => UpdateMethodologyStatus(methodology, request))
                .OnSuccess(methodology => UpdateMethodologyDetails(methodology, request))
                .OnSuccess(_ => GetSummary(id));
        }

        private async Task<Either<ActionResult, Methodology>> UpdateMethodologyStatus(Methodology methodologyToUpdate,
            MethodologyUpdateRequest request)
        {
            if (!request.IsStatusUpdateForMethodology(methodologyToUpdate))
            {
                // Status unchanged
                return methodologyToUpdate;
            }

            return await CheckCanUpdateMethodologyStatus(methodologyToUpdate, request.Status)
                .OnSuccessDo(methodology => CheckMethodologyCanDependOnRelease(methodology, request))
                .OnSuccessDo(RemoveUnusedImages)
                .OnSuccess(async methodology =>
                {
                    methodology.Status = request.Status;
                    methodology.PublishingStrategy = request.PublishingStrategy;
                    methodology.ScheduledWithReleaseId = request.WithReleaseId;
                    methodology.InternalReleaseNote = Approved == request.Status
                        ? request.LatestInternalReleaseNote
                        : null;

                    methodology.Updated = DateTime.UtcNow;

                    _context.Methodologies.Update(methodology);

                    if (await _methodologyRepository.IsPubliclyAccessible(methodology.Id))
                    {
                        methodology.Published = DateTime.UtcNow;

                        await _publishingService.PublishMethodologyFiles(methodology.Id);

                        // Invalidate the 'All Methodologies' cache item
                        await _blobCacheService.DeleteItem(new AllMethodologiesCacheKey());
                    }

                    await _context.SaveChangesAsync();
                    return methodology;
                });
        }

        private async Task<Either<ActionResult, Methodology>> UpdateMethodologyDetails(Methodology methodologyToUpdate,
            MethodologyUpdateRequest request)
        {
            if (!request.IsDetailUpdateForMethodology(methodologyToUpdate))
            {
                // Details unchanged
                return methodologyToUpdate;
            }

            return await _userService.CheckCanUpdateMethodology(methodologyToUpdate)
                // Check that the Methodology will have a unique slug.  It is possible to have a clash in the case where
                // another Methodology has previously set its AlternativeTitle (and Slug) to something specific and then
                // this Methodology attempts to set its AlternativeTitle (and Slug) to the same value.  Whilst an
                // unlikely scenario, it's entirely possible. 
                .OnSuccessDo(methodology => ValidateMethodologySlugUniqueForUpdate(methodology.Id, request.Slug))
                .OnSuccess(async methodology =>
                {
                    methodology.Updated = DateTime.UtcNow;

                    if (request.Title != methodology.Title)
                    {
                        methodology.AlternativeTitle =
                            request.Title != methodology.MethodologyParent.OwningPublicationTitle
                                ? request.Title
                                : null;

                        // If we're updating a Methodology that is not an Amendment, it's not yet publicly
                        // visible and so its Slug can be updated.  At the point that a Methodology is publicly
                        // visible and the only means of updating it is via Amendments, we will no longer allow its
                        // Slug to change even though its AlternativeTitle can.
                        if (!methodology.Amendment)
                        {
                            methodology.MethodologyParent.Slug = request.Slug;
                        }
                    }

                    await _context.SaveChangesAsync();

                    return methodology;
                });
        }

        public Task<Either<ActionResult, Unit>> DeleteMethodology(Guid methodologyId, bool forceDelete = false)
        {
            return _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId,
                    query => query.Include(m => m.MethodologyParent))
                .OnSuccess(methodology => _userService.CheckCanDeleteMethodology(methodology, forceDelete))
                .OnSuccessDo(UnlinkMethodologyFilesAndDeleteIfOrphaned)
                .OnSuccessDo(DeleteMethodologyVersion)
                .OnSuccessVoid(DeleteMethodologyParentIfOrphaned);
        }

        private async Task<Either<ActionResult, Unit>> UnlinkMethodologyFilesAndDeleteIfOrphaned(
            Methodology methodology)
        {
            var methodologyFileIds = await _context
                .MethodologyFiles
                .Where(f => f.MethodologyId == methodology.Id)
                .Select(f => f.FileId)
                .ToListAsync();

            if (methodologyFileIds.Count > 0)
            {
                return await _methodologyImageService.Delete(methodology.Id, methodologyFileIds);
            }

            return Unit.Instance;
        }

        private async Task DeleteMethodologyVersion(Methodology methodology)
        {
            _context.Methodologies.Remove(methodology);
            await _context.SaveChangesAsync();
        }

        private async Task DeleteMethodologyParentIfOrphaned(Methodology methodology)
        {
            var methodologyParent = await _context
                .MethodologyParents
                .Include(p => p.Versions)
                .SingleAsync(p => p.Id == methodology.MethodologyParentId);

            if (methodologyParent.Versions.Count == 0)
            {
                _context.MethodologyParents.Remove(methodologyParent);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<Either<ActionResult, Unit>> CheckMethodologyCanDependOnRelease(
            Methodology methodology,
            MethodologyUpdateRequest request)
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
            return await _persistenceHelper.CheckEntityExists<Release>(request.WithReleaseId.Value)
                .OnSuccess<ActionResult, Release, Unit>(async release =>
                {
                    if (release.Live)
                    {
                        return ValidationActionResult(MethodologyCannotDependOnPublishedRelease);
                    }

                    await _context.Entry(methodology)
                        .Reference(m => m.MethodologyParent)
                        .LoadAsync();

                    await _context.Entry(methodology.MethodologyParent)
                        .Collection(mp => mp.Publications)
                        .LoadAsync();

                    var publicationIds = methodology.MethodologyParent.Publications
                        .Select(pm => pm.PublicationId)
                        .ToList();

                    if (!publicationIds.Contains(release.PublicationId))
                    {
                        return ValidationActionResult(MethodologyCannotDependOnRelease);
                    }

                    return Unit.Instance;
                });
        }

        private Task<Either<ActionResult, Methodology>> CheckCanUpdateMethodologyStatus(Methodology methodology,
            MethodologyStatus requestedStatus)
        {
            return requestedStatus switch
            {
                Draft => _userService.CheckCanMarkMethodologyAsDraft(methodology),
                Approved => _userService.CheckCanApproveMethodology(methodology),
                _ => throw new ArgumentOutOfRangeException(nameof(requestedStatus), "Unexpected status")
            };
        }

        private async Task<Either<ActionResult, Unit>> RemoveUnusedImages(Methodology methodology)
        {
            return await _methodologyContentService.GetContentBlocks<HtmlBlock>(methodology.Id)
                .OnSuccess(async contentBlocks =>
                {
                    var contentImageIds = contentBlocks.SelectMany(contentBlock =>
                            HtmlImageUtil.GetMethodologyImages(contentBlock.Body))
                        .Distinct();

                    var imageFiles = await _methodologyFileRepository.GetByFileType(methodology.Id, Image);

                    var unusedImages = imageFiles
                        .Where(file => !contentImageIds.Contains(file.File.Id))
                        .Select(file => file.File.Id)
                        .ToList();

                    if (unusedImages.Any())
                    {
                        return await _methodologyImageService.Delete(methodology.Id, unusedImages);
                    }

                    return Unit.Instance;
                });
        }

        private static TitleAndIdViewModel BuildPublicationViewModel(PublicationMethodology publicationMethodology)
        {
            var publication = publicationMethodology.Publication;
            return new TitleAndIdViewModel(publication.Id, publication.Title);
        }

        private async Task<Either<ActionResult, Unit>> ValidateMethodologySlugUniqueForUpdate(Guid id, string slug)
        {
            var methodologyParentId = await _context
                .Methodologies
                .Where(m => m.Id == id)
                .Select(m => m.MethodologyParentId)
                .SingleAsync();

            if (await _context
                .MethodologyParents
                .AnyAsync(p => p.Slug == slug && p.Id != methodologyParentId))
            {
                return ValidationActionResult(SlugNotUnique);
            }

            return Unit.Instance;
        }

        private static IIncludableQueryable<Methodology, Publication> HydrateMethodologyForMethodologySummaryViewModel(
            IQueryable<Methodology> queryable)
        {
            return queryable
                .Include(methodology => methodology.MethodologyParent)
                .ThenInclude(mp => mp.Publications)
                .ThenInclude(pm => pm.Publication);
        }

        private static IQueryable<Methodology> HydrateMethodologyForManageMethodologySummaryViewModel(
            IQueryable<Methodology> queryable)
        {
            return queryable.Include(methodology => methodology.MethodologyParent);
        }
    }
}
