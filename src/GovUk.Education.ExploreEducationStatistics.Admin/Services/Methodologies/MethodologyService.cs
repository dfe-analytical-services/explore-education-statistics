﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class MethodologyService : IMethodologyService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMethodologyVersionRepository _methodologyVersionRepository;
        private readonly IMethodologyRepository _methodologyRepository;
        private readonly IMethodologyImageService _methodologyImageService;
        private readonly IMethodologyApprovalService _methodologyApprovalService;
        private readonly IUserService _userService;

        public MethodologyService(
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            ContentDbContext context,
            IMapper mapper,
            IMethodologyVersionRepository methodologyVersionRepository,
            IMethodologyRepository methodologyRepository,
            IMethodologyImageService methodologyImageService,
            IMethodologyApprovalService methodologyApprovalService,
            IUserService userService)
        {
            _persistenceHelper = persistenceHelper;
            _context = context;
            _mapper = mapper;
            _methodologyVersionRepository = methodologyVersionRepository;
            _methodologyRepository = methodologyRepository;
            _methodologyImageService = methodologyImageService;
            _methodologyApprovalService = methodologyApprovalService;
            _userService = userService;
        }

        public async Task<Either<ActionResult, Unit>> AdoptMethodology(Guid publicationId, Guid methodologyId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId, q =>
                    q.Include(p => p.Methodologies))
                .OnSuccess(_userService.CheckCanAdoptMethodologyForPublication)
                .OnSuccessDo(_ => _persistenceHelper.CheckEntityExists<Methodology>(methodologyId))
                .OnSuccess<ActionResult, Publication, Unit>(async publication =>
                {
                    if (publication.Methodologies.Any(pm => pm.MethodologyId == methodologyId))
                    {
                        return ValidationActionResult(CannotAdoptMethodologyAlreadyLinkedToPublication);
                    }

                    publication.Methodologies.Add(new PublicationMethodology
                    {
                        MethodologyId = methodologyId,
                        Owner = false
                    });

                    _context.Publications.Update(publication);
                    await _context.SaveChangesAsync();

                    return Unit.Instance;
                });
        }

        public Task<Either<ActionResult, MethodologyVersionSummaryViewModel>> CreateMethodology(Guid publicationId)
        {
            return _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccess(_userService.CheckCanCreateMethodologyForPublication)
                .OnSuccess(() => _methodologyVersionRepository
                    .CreateMethodologyForPublication(publicationId, _userService.GetUserId())
                )
                .OnSuccess(BuildMethodologySummaryViewModel);
        }

        public async Task<Either<ActionResult, Unit>> DropMethodology(Guid publicationId, Guid methodologyId)
        {
            return await _persistenceHelper
                .CheckEntityExists<PublicationMethodology>(q =>
                    q.Where(link => link.PublicationId == publicationId
                                    && link.MethodologyId == methodologyId))
                .OnSuccess(link => _userService.CheckCanDropMethodologyLink(link))
                .OnSuccessVoid(async link =>
                {
                    _context.PublicationMethodologies.Remove(link);
                    await _context.SaveChangesAsync();
                });
        }

        public async Task<Either<ActionResult, List<MethodologyVersionSummaryViewModel>>> GetAdoptableMethodologies(
            Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccess(_userService.CheckCanAdoptMethodologyForPublication)
                .OnSuccess(async publication =>
                {
                    var methodologies = await _methodologyRepository.GetUnrelatedToPublication(publication.Id);
                    var latestVersions = await methodologies.SelectAsync(methodology =>
                        _methodologyVersionRepository.GetLatestVersion(methodology.Id));
                    return (await latestVersions.SelectAsync(BuildMethodologySummaryViewModel)).ToList();
                });
        }

        public async Task<Either<ActionResult, MethodologyVersionSummaryViewModel>> GetSummary(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(id)
                .OnSuccess(_userService.CheckCanViewMethodology)
                .OnSuccess(BuildMethodologySummaryViewModel);
        }

        public async Task<Either<ActionResult, List<TitleAndIdViewModel>>> GetUnpublishedReleasesUsingMethodology(
            Guid methodologyVersionId)
        {
            return await _persistenceHelper.CheckEntityExists<MethodologyVersion>(methodologyVersionId, queryable =>
                    queryable.Include(m => m.Methodology)
                        .ThenInclude(mp => mp.Publications))
                .OnSuccess(methodologyVersion => _userService.CheckCanApproveMethodology(methodologyVersion)
                    .OrElse(() => _userService.CheckCanMarkMethodologyAsDraft(methodologyVersion)))
                .OnSuccess(async methodologyVersion =>
                {
                    // Get all Publications using the Methodology including adopting Publications
                    var publicationIds =
                        methodologyVersion.Methodology.Publications.Select(pm => pm.PublicationId);

                    // Get the Releases of those publications
                    var releases = await _context.Releases
                        .Include(r => r.Publication)
                        .Where(r => publicationIds.Contains(r.PublicationId))
                        .ToListAsync();

                    // Return an ordered list of the Releases that are not published
                    return releases.Where(r => !r.Live)
                        .OrderBy(r => r.Publication.Title)
                        .ThenByDescending(r => r.Year)
                        .ThenByDescending(r => r.TimePeriodCoverage)
                        .Select(r => new TitleAndIdViewModel(r.Id, $"{r.Publication.Title} - {r.Title}"))
                        .ToList();
                });
        }

        public async Task<Either<ActionResult, MethodologyVersionSummaryViewModel>> UpdateMethodology(Guid id,
            MethodologyUpdateRequest request)
        {
            return await _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(id, q =>
                    q.Include(m => m.Methodology))
                .OnSuccess(methodology => UpdateStatus(methodology, request))
                .OnSuccess(methodology => UpdateDetails(methodology, request))
                .OnSuccess(_ => GetSummary(id));
        }

        private async Task<MethodologyVersionSummaryViewModel> BuildMethodologySummaryViewModel(
            MethodologyVersion methodologyVersion)
        {
            var loadedMethodology = _context.AssertEntityLoaded(methodologyVersion);
            await _context.Entry(loadedMethodology)
                .Reference(m => m.Methodology)
                .Query()
                .Include(m => m.Publications)
                .ThenInclude(p => p.Publication)
                .LoadAsync();

            var publicationLinks = loadedMethodology.Methodology.Publications;
            var owningPublication = BuildPublicationViewModel(publicationLinks.Single(pm => pm.Owner));
            var otherPublications = publicationLinks.Where(pm => !pm.Owner)
                .Select(BuildPublicationViewModel)
                .OrderBy(model => model.Title)
                .ToList();

            var viewModel = _mapper.Map<MethodologyVersionSummaryViewModel>(loadedMethodology);

            viewModel.OwningPublication = owningPublication;
            viewModel.OtherPublications = otherPublications;

            if (loadedMethodology.ScheduledForPublishingWithRelease)
            {
                await _context.Entry(loadedMethodology)
                    .Reference(m => m.ScheduledWithRelease)
                    .LoadAsync();

                if (loadedMethodology.ScheduledWithRelease != null)
                {
                    await _context.Entry(loadedMethodology.ScheduledWithRelease)
                        .Reference(r => r!.Publication)
                        .LoadAsync();

                    var title =
                        $"{loadedMethodology.ScheduledWithRelease.Publication.Title} - {loadedMethodology.ScheduledWithRelease.Title}";
                    viewModel.ScheduledWithRelease = new TitleAndIdViewModel(
                        loadedMethodology.ScheduledWithRelease.Id,
                        title);
                }
            }

            return viewModel;
        }

        private async Task<Either<ActionResult, MethodologyVersion>> UpdateStatus(
            MethodologyVersion methodologyVersionToUpdate,
            MethodologyApprovalUpdateRequest request)
        {
            if (!request.IsStatusUpdateForMethodology(methodologyVersionToUpdate))
            {
                return methodologyVersionToUpdate;
            }
            
            return await _methodologyApprovalService
                .UpdateApprovalStatus(methodologyVersionToUpdate.Id, request)
                .OnSuccess(_ => _context
                    .MethodologyVersions
                    .Include(mv => mv.Methodology)
                    .SingleAsync(mv => mv.Id == methodologyVersionToUpdate.Id));
        }

        private async Task<Either<ActionResult, MethodologyVersion>> UpdateDetails(
            MethodologyVersion methodologyVersionToUpdate,
            MethodologyUpdateRequest request)
        {
            if (!request.IsDetailUpdateForMethodology(methodologyVersionToUpdate))
            {
                // Details unchanged
                return methodologyVersionToUpdate;
            }

            return await _userService.CheckCanUpdateMethodology(methodologyVersionToUpdate)
                // Check that the Methodology will have a unique slug.  It is possible to have a clash in the case where
                // another Methodology has previously set its AlternativeTitle (and Slug) to something specific and then
                // this Methodology attempts to set its AlternativeTitle (and Slug) to the same value.  Whilst an
                // unlikely scenario, it's entirely possible.
                .OnSuccessDo(methodologyVersion =>
                    ValidateMethodologySlugUniqueForUpdate(methodologyVersion.Id, request.Slug))
                .OnSuccess(async methodologyVersion =>
                {
                    methodologyVersion.Updated = DateTime.UtcNow;

                    if (request.Title != methodologyVersion.Title)
                    {
                        methodologyVersion.AlternativeTitle =
                            request.Title != methodologyVersion.Methodology.OwningPublicationTitle
                                ? request.Title
                                : null;

                        // If we're updating a Methodology that is not an Amendment, it's not yet publicly
                        // visible and so its Slug can be updated.  At the point that a Methodology is publicly
                        // visible and the only means of updating it is via Amendments, we will no longer allow its
                        // Slug to change even though its AlternativeTitle can.
                        if (!methodologyVersion.Amendment)
                        {
                            methodologyVersion.Methodology.Slug = request.Slug;
                        }
                    }

                    await _context.SaveChangesAsync();

                    return methodologyVersion;
                });
        }

        public async Task<Either<ActionResult, Unit>> DeleteMethodology(Guid methodologyId, bool forceDelete = false)
        {
            return await _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId,
                    query => query.Include(m => m.Versions))
                .OnSuccess(async methodology =>
                {
                    var methodologyVersionIds= methodology
                        .Versions
                        .Select(methodologyVersion => new IdAndPreviousVersionIdPair<string>(
                                methodologyVersion.Id.ToString(),
                                methodologyVersion.PreviousVersionId?.ToString()))
                        .ToList();

                    var methodologyVersionIdsInDeleteOrder = VersionedEntityDeletionOrderUtil
                        .Sort(methodologyVersionIds)
                        .Select(ids => Guid.Parse(ids.Id));
                        
                    return await methodologyVersionIdsInDeleteOrder    
                        .Select(methodologyVersionId => methodology.Versions.Single(version => version.Id == methodologyVersionId))
                        .Select(methodologyVersion => DeleteVersion(methodologyVersion, forceDelete))
                        .OnSuccessAll()
                        .OnSuccessVoid(async () =>
                        {
                            _context.Methodologies.Remove(methodology);
                            await _context.SaveChangesAsync();
                        });
                });
        }

        public Task<Either<ActionResult, Unit>> DeleteMethodologyVersion(
            Guid methodologyVersionId,
            bool forceDelete = false)
        {
            return _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(methodologyVersionId,
                    query => query.Include(m => m.Methodology))
                .OnSuccessDo(methodologyVersion => DeleteVersion(methodologyVersion, forceDelete))
                .OnSuccessVoid(DeleteMethodologyIfOrphaned);
        }

        private async Task<Either<ActionResult, Unit>> DeleteVersion(MethodologyVersion methodologyVersion,
            bool forceDelete = false)
        {
            return await _userService.CheckCanDeleteMethodology(methodologyVersion, forceDelete)
                .OnSuccess(() => _methodologyImageService.DeleteAll(methodologyVersion.Id, forceDelete))
                .OnSuccessVoid(async () =>
                {
                    _context.MethodologyVersions.Remove(methodologyVersion);
                    await _context.SaveChangesAsync();
                });
        }

        private async Task DeleteMethodologyIfOrphaned(MethodologyVersion methodologyVersion)
        {
            var methodology = await _context
                .Methodologies
                .Include(p => p.Versions)
                .SingleAsync(p => p.Id == methodologyVersion.MethodologyId);

            if (methodology.Versions.Count == 0)
            {
                _context.Methodologies.Remove(methodology);
                await _context.SaveChangesAsync();
            }
        }

        private static TitleAndIdViewModel BuildPublicationViewModel(PublicationMethodology publicationMethodology)
        {
            var publication = publicationMethodology.Publication;
            return new TitleAndIdViewModel(publication.Id, publication.Title);
        }

        private async Task<Either<ActionResult, Unit>> ValidateMethodologySlugUniqueForUpdate(
            Guid methodologyVersionId, string slug)
        {
            var methodologyId = await _context
                .MethodologyVersions
                .AsQueryable()
                .Where(m => m.Id == methodologyVersionId)
                .Select(m => m.MethodologyId)
                .SingleAsync();

            if (await _context
                .Methodologies
                .AsQueryable()
                .AnyAsync(p => p.Slug == slug && p.Id != methodologyId))
            {
                return ValidationActionResult(SlugNotUnique);
            }

            return Unit.Instance;
        }
    }
}
