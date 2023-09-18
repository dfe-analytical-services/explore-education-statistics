﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
        private readonly IMethodologyCacheService _methodologyCacheService;
        private readonly IUserService _userService;

        public MethodologyService(
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            ContentDbContext context,
            IMapper mapper,
            IMethodologyVersionRepository methodologyVersionRepository,
            IMethodologyRepository methodologyRepository,
            IMethodologyImageService methodologyImageService,
            IMethodologyApprovalService methodologyApprovalService,
            IMethodologyCacheService methodologyCacheService,
            IUserService userService)
        {
            _persistenceHelper = persistenceHelper;
            _context = context;
            _mapper = mapper;
            _methodologyVersionRepository = methodologyVersionRepository;
            _methodologyRepository = methodologyRepository;
            _methodologyImageService = methodologyImageService;
            _methodologyApprovalService = methodologyApprovalService;
            _methodologyCacheService = methodologyCacheService;
            _userService = userService;
        }

        public async Task<Either<ActionResult, Unit>> AdoptMethodology(Guid publicationId, Guid methodologyId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId, q =>
                    q.Include(p => p.Methodologies))
                .OnSuccess(_userService.CheckCanAdoptMethodologyForPublication)
                .OnSuccessCombineWith(_ => _persistenceHelper.CheckEntityExists<Methodology>(methodologyId))
                .OnSuccess<ActionResult, Tuple<Publication, Methodology>, Unit>(async tuple =>
                {
                    var (publication, methodology) = tuple;

                    if (methodology.LatestPublishedVersionId == null)
                    {
                        throw new ArgumentException("Cannot adopt an unpublished methodology");
                    }

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

                    await _methodologyCacheService.UpdateSummariesTree();

                    return Unit.Instance;
                });
        }

        public Task<Either<ActionResult, MethodologyVersionViewModel>> CreateMethodology(Guid publicationId)
        {
            return _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccess(_userService.CheckCanCreateMethodologyForPublication)
                .OnSuccess(() => _methodologyVersionRepository
                    .CreateMethodologyForPublication(publicationId, _userService.GetUserId())
                )
                .OnSuccess(BuildMethodologyVersionViewModel);
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

                    await _methodologyCacheService.UpdateSummariesTree();
                });
        }

        public async Task<Either<ActionResult, List<MethodologyVersionViewModel>>> GetAdoptableMethodologies(
            Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccess(_userService.CheckCanAdoptMethodologyForPublication)
                .OnSuccess(async publication =>
                {
                    var publishedMethodologies = await _methodologyRepository
                        .GetPublishedMethodologiesUnrelatedToPublication(publication.Id);
                    var latestPublishedVersions = publishedMethodologies
                        .ToAsyncEnumerable()
                        .SelectAwait(async methodology =>
                            await _methodologyVersionRepository.GetLatestPublishedVersion(methodology.Id))
                        .WhereNotNull();
                    return await latestPublishedVersions
                        .SelectAwait(async version => await BuildMethodologyVersionViewModel(version))
                        .ToListAsync();
                });
        }

        public async Task<Either<ActionResult, MethodologyVersionViewModel>> GetMethodology(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(id)
                .OnSuccess(_userService.CheckCanViewMethodology)
                .OnSuccess(BuildMethodologyVersionViewModel);
        }

        public async Task<Either<ActionResult, List<MethodologyVersionSummaryViewModel>>> ListLatestMethodologyVersions(Guid publicationId)
        {
            return await _persistenceHelper.CheckEntityExists<Publication>(publicationId,
                    q => q.Include(p => p.Methodologies)
                        .ThenInclude(p => p.Methodology)
                        .ThenInclude(p => p.Versions))
                .OnSuccess(publication => _userService.CheckCanViewPublication(publication))
                .OnSuccess(async publication =>
                {
                    return await publication.Methodologies
                        .ToAsyncEnumerable()
                        .SelectAwait(async publicationMethodology =>
                        {
                            var latestVersion = publicationMethodology.Methodology.LatestVersion();
                            var permissions =
                                await PermissionsUtils.GetMethodologyVersionPermissions(_userService,
                                    latestVersion,
                                    publicationMethodology);

                            return new MethodologyVersionSummaryViewModel
                            {
                                Id = latestVersion.Id,
                                Amendment = latestVersion.Amendment,
                                Owned = publicationMethodology.Owner,
                                Published = latestVersion.Published,
                                Status = latestVersion.Status,
                                Title = latestVersion.Title,
                                MethodologyId = latestVersion.MethodologyId,
                                PreviousVersionId = latestVersion.PreviousVersionId,
                                Permissions = permissions,
                            };
                        })
                        .OrderBy(viewModel => viewModel.Title)
                        .ToListAsync();
                });
        }

        public async Task<Either<ActionResult, List<IdTitleViewModel>>> GetUnpublishedReleasesUsingMethodology(
            Guid methodologyVersionId)
        {
            return await _persistenceHelper.CheckEntityExists<MethodologyVersion>(methodologyVersionId, queryable =>
                    queryable.Include(m => m.Methodology)
                        .ThenInclude(mp => mp.Publications))
                .OnSuccess(methodologyVersion => _userService.CheckCanApproveMethodologyVersion(methodologyVersion)
                    .OrElse(() => _userService.CheckCanMarkMethodologyVersionAsDraft(methodologyVersion)))
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
                        .Select(r => new IdTitleViewModel(r.Id, $"{r.Publication.Title} - {r.Title}"))
                        .ToList();
                });
        }

        public async Task<Either<ActionResult, MethodologyVersionViewModel>> UpdateMethodology(Guid id,
            MethodologyUpdateRequest request)
        {
            return await _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(id, q =>
                    q.Include(m => m.Methodology))
                .OnSuccess(methodologyVersion => UpdateStatus(methodologyVersion, request))
                .OnSuccess(methodologyVersion => UpdateDetails(methodologyVersion, request))
                .OnSuccess(BuildMethodologyVersionViewModel);
        }

        public async Task<MethodologyVersionViewModel> BuildMethodologyVersionViewModel(
            MethodologyVersion methodologyVersion)
        {
            var loadedMethodologyVersion = _context.AssertEntityLoaded(methodologyVersion);
            await _context.Entry(loadedMethodologyVersion)
                .Reference(m => m.Methodology)
                .Query()
                .Include(m => m.Publications)
                .ThenInclude(p => p.Publication)
                .LoadAsync();

            var publicationLinks = loadedMethodologyVersion.Methodology.Publications;
            var owningPublication = BuildPublicationViewModel(publicationLinks.Single(pm => pm.Owner));
            var otherPublications = publicationLinks.Where(pm => !pm.Owner)
                .Select(BuildPublicationViewModel)
                .OrderBy(model => model.Title)
                .ToList();

            var viewModel = _mapper.Map<MethodologyVersionViewModel>(loadedMethodologyVersion);

            viewModel.InternalReleaseNote = await GetLatestInternalNote(loadedMethodologyVersion);

            viewModel.OwningPublication = owningPublication;
            viewModel.OtherPublications = otherPublications;

            if (loadedMethodologyVersion.ScheduledForPublishingWithRelease)
            {
                await _context.Entry(loadedMethodologyVersion)
                    .Reference(m => m.ScheduledWithRelease)
                    .LoadAsync();

                if (loadedMethodologyVersion.ScheduledWithRelease != null)
                {
                    await _context.Entry(loadedMethodologyVersion.ScheduledWithRelease)
                        .Reference(r => r!.Publication)
                        .LoadAsync();

                    var title =
                        $"{loadedMethodologyVersion.ScheduledWithRelease.Publication.Title} - {loadedMethodologyVersion.ScheduledWithRelease.Title}";
                    viewModel.ScheduledWithRelease = new IdTitleViewModel(
                        loadedMethodologyVersion.ScheduledWithRelease.Id,
                        title);
                }
            }

            return viewModel;
        }

        private async Task<Either<ActionResult, MethodologyVersion>> UpdateStatus(
            MethodologyVersion methodologyVersionToUpdate,
            MethodologyApprovalUpdateRequest request)
        {
            if (!request.IsStatusUpdateRequired(methodologyVersionToUpdate))
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
            if (methodologyVersionToUpdate.Title == request.Title)
            {
                // Details unchanged
                return methodologyVersionToUpdate;
            }

            return await _userService.CheckCanUpdateMethodologyVersion(methodologyVersionToUpdate)
                // Check that the Methodology will have a unique slug.  It is possible to have a clash in the case where
                // another Methodology has previously set its AlternativeTitle (and Slug) to something specific and then
                // this Methodology attempts to set its AlternativeTitle (and Slug) to the same value.  Whilst an
                // unlikely scenario, it's entirely possible.
                .OnSuccessDo(methodologyVersion =>
                    ValidateMethodologySlugUniqueForUpdate(methodologyVersion, request.Slug))
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

        public Task<Either<ActionResult, List<MethodologyStatusViewModel>>> GetMethodologyStatuses(
            Guid methodologyVersionId)
        {
            return _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(methodologyVersionId)
                .OnSuccess(_userService.CheckCanViewMethodology)
                .OnSuccess(async methodologyVersion =>
                {
                    var methodologyVersionIds = await _context.MethodologyVersions
                        .Where(mv => mv.MethodologyId == methodologyVersion.MethodologyId)
                        .Select(mv => mv.Id)
                        .ToListAsync();

                    var statuses = await _context.MethodologyStatus
                        .Include(ms => ms.MethodologyVersion)
                        .Include(ms => ms.CreatedBy)
                        .Where(status => methodologyVersionIds.Contains(status.MethodologyVersionId))
                        .ToListAsync();

                    return statuses
                        .Select(status =>
                            new MethodologyStatusViewModel
                            {
                                MethodologyStatusId = status.Id,
                                InternalReleaseNote = status.InternalReleaseNote,
                                ApprovalStatus = status.ApprovalStatus,
                                Created = status.Created,
                                CreatedByEmail = status.CreatedBy?.Email,
                                MethodologyVersion = status.MethodologyVersion.Version,
                            }
                        )
                        .OrderByDescending(vm => vm.Created)
                        .ToList();
                });
        }

        private async Task<Either<ActionResult, Unit>> DeleteVersion(MethodologyVersion methodologyVersion,
            bool forceDelete = false)
        {
            return await _userService.CheckCanDeleteMethodologyVersion(methodologyVersion, forceDelete)
                .OnSuccess(() => _methodologyImageService.DeleteAll(methodologyVersion.Id, forceDelete))
                .OnSuccessVoid(async () =>
                {
                    _context.MethodologyVersions.Remove(methodologyVersion);
                    await _context.SaveChangesAsync();
                });
        }

        private async Task<string?> GetLatestInternalNote(MethodologyVersion methodologyVersion)
        {
            // NOTE: Gets latest internal note for this version, not for the entire methodology
            return await _context.MethodologyStatus
                .Where(ms => methodologyVersion.Id == ms.MethodologyVersionId)
                .OrderByDescending(ms => ms.Created)
                .Select(ms => ms.InternalReleaseNote)
                .FirstOrDefaultAsync();
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

        private static IdTitleViewModel BuildPublicationViewModel(PublicationMethodology publicationMethodology)
        {
            var publication = publicationMethodology.Publication;
            return new IdTitleViewModel(publication.Id, publication.Title);
        }

        private async Task<Either<ActionResult, Unit>> ValidateMethodologySlugUniqueForUpdate(
            MethodologyVersion methodologyVersion, string slug)
        {
            if (await _context
                    .Methodologies
                    .AsQueryable()
                    .AnyAsync(p => p.Slug == slug && p.Id != methodologyVersion.MethodologyId))
            {
                return ValidationActionResult(SlugNotUnique);
            }

            return Unit.Instance;
        }
    }
}
