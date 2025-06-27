#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;
using MethodologyVersionSummaryViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.MethodologyVersionSummaryViewModel;
using MethodologyVersionViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology.MethodologyVersionViewModel;

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
        private readonly IRedirectsCacheService _redirectsCacheService;
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
            IRedirectsCacheService redirectsCacheService,
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
            _redirectsCacheService = redirectsCacheService;
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
                .OnSuccess(publication => ValidateMethodologySlug(publication.Slug))
                .OnSuccess(_ => _methodologyVersionRepository
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

        public async Task<Either<ActionResult, List<MethodologyVersionSummaryViewModel>>>
            ListLatestMethodologyVersions(
                Guid publicationId,
                bool isPrerelease = false)
        {
            return await _persistenceHelper.CheckEntityExists<Publication>(publicationId,
                    q => q.Include(publication => publication.Methodologies)
                        .ThenInclude(publicationMethodology => publicationMethodology.Methodology)
                        .ThenInclude(methodology => methodology.Versions)
                        .ThenInclude(versions => versions.PreviousVersion))
                .OnSuccess(publication => _userService.CheckCanViewPublication(publication))
                .OnSuccess(async publication =>
                {
                    return await publication.Methodologies
                        .ToAsyncEnumerable()
                        .SelectAwait(async publicationMethodology =>
                        {
                            var methodologyVersion = publicationMethodology.Methodology.LatestVersion();

                            if (isPrerelease && methodologyVersion.Status != MethodologyApprovalStatus.Approved)
                            {
                                // Get latest approved version
                                if (methodologyVersion.PreviousVersion == null)
                                {
                                    return null;
                                }

                                // If there is a previous version, it must be approved, because cannot
                                // create an amendment for an unpublished version
                                methodologyVersion = methodologyVersion.PreviousVersion;
                            }

                            var permissions =
                                await PermissionsUtils.GetMethodologyVersionPermissions(_userService,
                                    methodologyVersion,
                                    publicationMethodology);

                            return new MethodologyVersionSummaryViewModel
                            {
                                Id = methodologyVersion.Id,
                                Amendment = methodologyVersion.Amendment,
                                Owned = publicationMethodology.Owner,
                                Published = methodologyVersion.Published,
                                Status = methodologyVersion.Status,
                                Title = methodologyVersion.Title,
                                MethodologyId = methodologyVersion.MethodologyId,
                                PreviousVersionId = methodologyVersion.PreviousVersionId,
                                Permissions = permissions,
                            };
                        })
                        .WhereNotNull()
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
                    var releaseVersions = await _context.ReleaseVersions
                        .Include(rv => rv.Release)
                        .ThenInclude(r => r.Publication)
                        .Where(rv => publicationIds.Contains(rv.PublicationId))
                        .ToListAsync();

                    // Return an ordered list of the Releases that are not published
                    return releaseVersions.Where(rv => !rv.Live)
                        .OrderBy(rv => rv.Release.Publication.Title)
                        .ThenByDescending(rv => rv.Release.Year)
                        .ThenByDescending(rv => rv.Release.TimePeriodCoverage)
                        .Select(rv => new IdTitleViewModel(rv.Id, $"{rv.Release.Publication.Title} - {rv.Release.Title}"))
                        .ToList();
                });
        }

        public async Task<Either<ActionResult, MethodologyVersionViewModel>> UpdateMethodology(Guid id,
            MethodologyUpdateRequest request)
        {
            return await _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(id, q =>
                    q.Include(m => m.Methodology))
                // NOTE: Permission checks nested within UpdateStatus and UpdateDetails
                .OnSuccess(methodologyVersion => UpdateStatus(methodologyVersion, request))
                .OnSuccess(methodologyVersion => UpdateDetails(methodologyVersion, request))
                .OnSuccess(BuildMethodologyVersionViewModel);
        }

        public async Task<Either<ActionResult, Unit>> UpdateMethodologyPublished(Guid methodologyVersionId,
            MethodologyPublishedUpdateRequest request)
        {
            return await _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(methodologyVersionId)
                .OnSuccessDo(_userService.CheckIsBauUser)
                .OnSuccess<ActionResult, MethodologyVersion, Unit>(async methodologyVersion =>
                {
                    if (methodologyVersion.Published == null)
                    {
                        return ValidationActionResult(MethodologyNotPublished);
                    }

                    var newPublishedDate = request.Published?.ToUniversalTime() ?? DateTime.UtcNow;

                    // Prevent assigning a future date since it would have the effect of un-publishing the methodology
                    if (newPublishedDate > DateTime.UtcNow)
                    {
                        return ValidationActionResult(MethodologyPublishedCannotBeFutureDate);
                    }

                    _context.MethodologyVersions.Update(methodologyVersion);
                    methodologyVersion.Published = newPublishedDate;
                    await _context.SaveChangesAsync();

                    // Update the cached methodology
                    await _methodologyCacheService.UpdateSummariesTree();

                    return Unit.Instance;
                });
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
                .ThenInclude(p => p.Contact)
                .Include(m => m.Publications)
                .ThenInclude(p => p.Publication)
                .ThenInclude(p => p.LatestPublishedReleaseVersion)
                .ThenInclude(p => p!.Release)
                .LoadAsync();

            var publicationLinks = loadedMethodologyVersion.Methodology.Publications;
            var owningPublication = BuildPublicationViewModel(publicationLinks.Single(pm => pm.Owner));
            var otherPublications = publicationLinks.Where(pm => !pm.Owner)
                .Select(BuildPublicationViewModel)
                .OrderBy(model => model.Title)
                .ToList();

            var viewModel = _mapper.Map<MethodologyVersionViewModel>(loadedMethodologyVersion);

            viewModel.InternalReleaseNote = await GetLatestInternalReleaseNote(loadedMethodologyVersion.Id);

            viewModel.OwningPublication = owningPublication;
            viewModel.OtherPublications = otherPublications;

            if (loadedMethodologyVersion.ScheduledForPublishingWithRelease)
            {
                await _context.Entry(loadedMethodologyVersion)
                    .Reference(m => m.ScheduledWithReleaseVersion)
                    .Query()
                    .Include(rv => rv.Release)
                    .ThenInclude(r => r.Publication)
                    .LoadAsync();

                if (loadedMethodologyVersion.ScheduledWithReleaseVersion != null)
                {
                    var title =
                        $"{loadedMethodologyVersion.ScheduledWithReleaseVersion.Release.Publication.Title} - {loadedMethodologyVersion.ScheduledWithReleaseVersion.Release.Title}";
                    viewModel.ScheduledWithRelease = new IdTitleViewModel(
                        loadedMethodologyVersion.ScheduledWithReleaseVersion.Id,
                        title);
                }
            }

            return viewModel;
        }

        private async Task<Either<ActionResult, MethodologyVersion>> UpdateStatus(
            MethodologyVersion methodologyVersionToUpdate,
            MethodologyUpdateRequest request)
        {
            if (!request.IsStatusUpdateRequired(methodologyVersionToUpdate))
            {
                return methodologyVersionToUpdate;
            }

            if (methodologyVersionToUpdate.Title != request.Title) // EES-3789 Should also check for slug change here too?
            {
                throw new ArgumentException(
                    "Should not update status of MethodologyVersion while simultaneously updating it's title");
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
            var newSlug = SlugFromTitle(request.Title);

            var titleChanged = methodologyVersionToUpdate.Title != request.Title;
            var slugChanged = methodologyVersionToUpdate.Slug != newSlug;

            if (!titleChanged && !slugChanged)
            {
                // Details unchanged
                return methodologyVersionToUpdate;
            }

            if (request.Status == MethodologyApprovalStatus.Approved)
            {
                throw new ArgumentException("Should not be updating details of an approved methodology");
            }

            return await _userService.CheckCanUpdateMethodologyVersion(methodologyVersionToUpdate)
                .OnSuccessDo(async methodologyVersion => await ValidateMethodologySlug(
                    newSlug,
                    oldSlug: methodologyVersionToUpdate.Slug,
                    methodologyId: methodologyVersion.MethodologyId))
                .OnSuccess(async methodologyVersion =>
                {
                    methodologyVersion.Updated = DateTime.UtcNow;

                    if (titleChanged)
                    {
                        methodologyVersion.AlternativeTitle =
                            request.Title != methodologyVersion.Methodology.OwningPublicationTitle
                                ? request.Title
                                : null;
                    }

                    if (slugChanged)
                    {
                        var methodology = methodologyVersion.Methodology;
                        await _context.Entry(methodology)
                            .Collection(m => m.Versions)
                            .LoadAsync();

                        if (methodology.LatestPublishedVersionId != null
                            && methodologyVersion.Amendment
                            && !await _context.MethodologyRedirects.AnyAsync(mr =>
                                // no redirect needed for an unpublished amendment that already has a redirect
                                // as the new redirect would be for a slug that hasn't been live
                                mr.MethodologyVersionId == methodologyVersion.Id
                                // don't create if a redirect for this slug already exists
                                || mr.Slug == methodologyVersion.Slug))
                        {
                            var methodologyRedirect = new MethodologyRedirect
                            {
                                MethodologyVersionId = methodologyVersion.Id,
                                Slug = methodologyVersion.Slug, // i.e. from the currently live slug
                            };
                            await _context.MethodologyRedirects.AddAsync(methodologyRedirect);
                        }

                        methodologyVersion.AlternativeSlug =
                            newSlug != methodologyVersion.Methodology.OwningPublicationSlug
                                ? newSlug
                                : null;
                    }

                    await _context.SaveChangesAsync();

                    // NOTE: No need to invalidate redirects.json cache here as the methodologyVersion is unpublished

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
                    var methodologyVersionIds = methodology
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

        public async Task<Either<ActionResult, List<MethodologyVersionViewModel>>> ListUsersMethodologyVersionsForApproval()
        {
            var userId = _userService.GetUserId();

            var directPublicationsWithApprovalRole = await _context
                .UserPublicationRoles
                .Where(role => role.UserId == userId && role.Role == PublicationRole.Allower)
                .Select(role => role.PublicationId)
                .ToListAsync();

            var indirectPublicationsWithApprovalRole = await _context
                .UserReleaseRoles
                .Where(role => role.UserId == userId && role.Role == ReleaseRole.Approver)
                .Select(role => role.ReleaseVersion.PublicationId)
                .ToListAsync();

            var publicationIdsForApproval = directPublicationsWithApprovalRole
                .Concat(indirectPublicationsWithApprovalRole)
                .Distinct();

            var methodologiesToApprove = await _context
                .MethodologyVersions
                .Where(methodologyVersion =>
                    methodologyVersion.Status == MethodologyApprovalStatus.HigherLevelReview
                    && methodologyVersion.Methodology.Publications.Any(
                        publicationMethodology =>
                            publicationMethodology.Owner
                            && publicationIdsForApproval.Contains(publicationMethodology.PublicationId)))
                .ToListAsync();

            return (await methodologiesToApprove
                    .SelectAsync(BuildMethodologyVersionViewModel))
                .ToList();
        }

        // This method is responsible for keeping methodology titles and slugs and associated redirects in sync with
        // their owning publications. Methodologies keep track of their owning publication's titles and slugs for
        // optimisation purposes.
        public async Task PublicationTitleOrSlugChanged(Guid publicationId, string originalSlug, string updatedTitle,
            string updatedSlug)
        {
            var slugChanged = originalSlug != updatedSlug;

            var ownedMethodology = await _context
                .PublicationMethodologies
                .Include(pm => pm.Methodology.LatestPublishedVersion)
                .Include(pm => pm.Methodology.Versions)
                .Where(pm => pm.PublicationId == publicationId && pm.Owner)
                .Select(pm => pm.Methodology)
                .SingleOrDefaultAsync();

            if (ownedMethodology == null)
            {
                return;
            }

            ownedMethodology.OwningPublicationTitle = updatedTitle;
            ownedMethodology.OwningPublicationSlug = updatedSlug;

            _context.Methodologies.Update(ownedMethodology);

            if (slugChanged)
            {
                var latestPublishedVersion = ownedMethodology.LatestPublishedVersion;

                // If the LatestPublishedVersion inherits the publication slug, it needs a redirect
                if (latestPublishedVersion is { AlternativeSlug: null })
                {
                    var redirect = new MethodologyRedirect
                    {
                        MethodologyVersion = latestPublishedVersion,
                        Slug = originalSlug,
                    };
                    _context.MethodologyRedirects.Add(redirect);

                    // If redirects already exists from updatedSlug - i.e. we're changing back to a slug we've used
                    // previously. The redirect is redundant, as we're now using that slug. So remove
                    var redundantRedirects = await _context.MethodologyRedirects
                        .Where(mr => mr.Slug == updatedSlug)
                        .ToListAsync();
                    if (redundantRedirects.Count > 0)
                    {
                        _context.MethodologyRedirects.RemoveRange(redundantRedirects);
                    }

                    // If we have an unpublished amendment with an AlternativeSlug set, then it'll have an
                    // inactive redirect from originalSlug. We remove that redirect - we've just created
                    // a redirect for originalSlug / LatestPublishedRelease - and create a new inactive
                    // redirect for the unpublished amendment from updatedSlug. We need this redirect for when
                    // the amendment is published and the new AlternativeSlug goes live.
                    var latestVersion = ownedMethodology.LatestVersion();
                    if (latestVersion.Id != latestPublishedVersion.Id
                        && latestVersion.AlternativeSlug != null)
                    {
                        var unpublishedAmendmentRedirectsToRemove = await _context.MethodologyRedirects
                            .Where(mr =>
                                mr.MethodologyVersionId == latestVersion.Id
                                && mr.Slug == originalSlug)
                            .ToListAsync();

                        if (unpublishedAmendmentRedirectsToRemove.Count > 0)
                        {
                            _context.MethodologyRedirects.RemoveRange(unpublishedAmendmentRedirectsToRemove);
                        }

                        var unpublishedAmendmentRedirect = new MethodologyRedirect
                        {
                            MethodologyVersion = latestVersion,
                            Slug = updatedSlug,
                        };
                        _context.MethodologyRedirects.Add(unpublishedAmendmentRedirect);
                    }
                }
            }

            await _context.SaveChangesAsync();

            if (slugChanged)
            {
                await _redirectsCacheService.UpdateRedirects();
            }
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

        private async Task<string?> GetLatestInternalReleaseNote(Guid methodologyVersionId)
        {
            // NOTE: Gets latest internal note for this version, not for the entire methodology
            return await _context.MethodologyStatus
                .Where(ms => methodologyVersionId == ms.MethodologyVersionId)
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

        private static PublicationSummaryViewModel BuildPublicationViewModel(PublicationMethodology publicationMethodology)
        {
            var publication = publicationMethodology.Publication;
            return new PublicationSummaryViewModel
            {
                Id = publication.Id,
                Title = publication.Title,
                Slug = publication.Slug,
                LatestReleaseSlug = publication.LatestPublishedReleaseVersion?.Release.Slug,
                Owner = publicationMethodology.Owner,
                Contact = new ContactViewModel(publication.Contact)
            };
        }

        public async Task<Either<ActionResult, Unit>> ValidateMethodologySlug(
            string newSlug, string? oldSlug = null, Guid? methodologyId = null)
        {
            if (newSlug == oldSlug)
            {
                // no slug change, no slug validation required
                return Unit.Instance;
            }

            var methodologyVersion = await _methodologyVersionRepository.GetLatestPublishedVersionBySlug(newSlug);

            if (methodologyVersion != null)
            {
                return ValidationActionResult(MethodologySlugNotUnique);
            }

            var redirectExistsToOtherMethodology = await _context.MethodologyRedirects
                .Where(mr =>
                    mr.Slug == newSlug
                    // we exclude redirects to the same methodology i.e. we allow the slug to be changed back
                    && (methodologyId != null && methodologyId != mr.MethodologyVersion.MethodologyId))
                .AnyAsync();

            if (redirectExistsToOtherMethodology)
            {
                return ValidationActionResult(MethodologySlugUsedByRedirect);
            }

            return Unit.Instance;
        }
    }
}
