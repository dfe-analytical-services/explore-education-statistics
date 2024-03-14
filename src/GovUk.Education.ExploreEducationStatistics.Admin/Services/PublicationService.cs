#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using ExternalMethodologyViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ExternalMethodologyViewModel;
using IPublicationRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationRepository;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;
using IPublicationService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationService;
using PublicationViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.PublicationViewModel;
using ReleaseSummaryViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ReleaseSummaryViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IPublicationRepository _publicationRepository;
        private readonly IReleaseVersionRepository _releaseVersionRepository;
        private readonly IMethodologyService _methodologyService;
        private readonly IPublicationCacheService _publicationCacheService;
        private readonly IMethodologyCacheService _methodologyCacheService;
        private readonly IRedirectsCacheService _redirectsCacheService;

        public PublicationService(
            ContentDbContext context,
            IMapper mapper,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            IPublicationRepository publicationRepository,
            IReleaseVersionRepository releaseVersionRepository,
            IMethodologyService methodologyService,
            IPublicationCacheService publicationCacheService,
            IMethodologyCacheService methodologyCacheService,
            IRedirectsCacheService redirectsCacheService)
        {
            _context = context;
            _mapper = mapper;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _publicationRepository = publicationRepository;
            _releaseVersionRepository = releaseVersionRepository;
            _methodologyService = methodologyService;
            _publicationCacheService = publicationCacheService;
            _methodologyCacheService = methodologyCacheService;
            _redirectsCacheService = redirectsCacheService;
        }

        public async Task<Either<ActionResult, List<PublicationViewModel>>> ListPublications(
            Guid? topicId = null)
        {
            return await _userService
                .CheckCanAccessSystem()
                .OnSuccess(_ => _userService.CheckCanViewAllPublications()
                    .OnSuccess(async () =>
                    {
                        var hydratedPublication = HydratePublication(
                            _publicationRepository.QueryPublicationsForTopic(topicId));
                        return await hydratedPublication.ToListAsync();
                    })
                    .OrElse(() =>
                    {
                        var userId = _userService.GetUserId();
                        return _publicationRepository.ListPublicationsForUser(userId, topicId);
                    })
                )
                .OnSuccess(async publications =>
                {
                    return await publications
                        .ToAsyncEnumerable()
                        .SelectAwait(async publication => await GeneratePublicationViewModel(publication))
                        .OrderBy(publicationViewModel => publicationViewModel.Title)
                        .ToListAsync();
                });
        }

        public async Task<Either<ActionResult, List<PublicationSummaryViewModel>>> ListPublicationSummaries()
        {
            return await _userService
                .CheckCanViewAllPublications()
                .OnSuccess(_ =>
                {
                    return _context.Publications
                        .Select(p => new PublicationSummaryViewModel
                        {
                            Id = p.Id,
                            Title = p.Title,
                            Slug = p.Slug,
                        })
                        .ToList();
                });
        }

        public async Task<Either<ActionResult, PublicationCreateViewModel>> CreatePublication(
            PublicationCreateRequest publication)
        {
            return await ValidateSelectedTopic(publication.TopicId)
                .OnSuccess(_ => ValidatePublicationSlug(publication.Slug))
                .OnSuccess(async _ =>
                {
                    var contact = await _context.Contacts.AddAsync(new Contact
                    {
                        ContactName = publication.Contact.ContactName,
                        ContactTelNo = string.IsNullOrWhiteSpace(publication.Contact.ContactTelNo)
                            ? null
                            : publication.Contact.ContactTelNo,
                        TeamName = publication.Contact.TeamName,
                        TeamEmail = publication.Contact.TeamEmail
                    });

                    var saved = await _context.Publications.AddAsync(new Publication
                    {
                        Contact = contact.Entity,
                        Title = publication.Title,
                        Summary = publication.Summary,
                        TopicId = publication.TopicId,
                        Slug = publication.Slug,
                    });

                    await _context.SaveChangesAsync();

                    return await _persistenceHelper
                        .CheckEntityExists<Publication>(saved.Entity.Id, HydratePublication)
                        .OnSuccess(GeneratePublicationCreateViewModel);
                });
        }

        public async Task<Either<ActionResult, PublicationViewModel>> UpdatePublication(
            Guid publicationId,
            PublicationSaveRequest updatedPublication)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccess(_userService.CheckCanUpdatePublicationSummary)
                .OnSuccessDo(async publication =>
                {
                    if (publication.Title != updatedPublication.Title)
                    {
                        return await _userService.CheckCanUpdatePublication();
                    }

                    return Unit.Instance;
                })
                .OnSuccessDo(async publication =>
                {
                    if (publication.SupersededById != updatedPublication.SupersededById)
                    {
                        return await _userService.CheckCanUpdatePublication();
                    }

                    return Unit.Instance;
                })
                .OnSuccessDo(async publication =>
                {
                    if (publication.TopicId != updatedPublication.TopicId)
                    {
                        return await ValidateSelectedTopic(updatedPublication.TopicId);
                    }

                    return Unit.Instance;
                })
                .OnSuccessDo(async publication =>
                {
                    if (publication.TopicId != updatedPublication.TopicId)
                    {
                        return await _userService.CheckCanUpdatePublication();
                    }

                    return Unit.Instance;
                })
                .OnSuccess(async publication =>
                {
                    var originalTitle = publication.Title;
                    var originalSlug = publication.Slug;

                    var titleChanged = originalTitle != updatedPublication.Title;
                    var slugChanged = originalSlug != updatedPublication.Slug;

                    if (slugChanged)
                    {
                        var slugValidation =
                            await ValidatePublicationSlug(updatedPublication.Slug, publication.Id);

                        if (slugValidation.IsLeft)
                        {
                            return new Either<ActionResult, PublicationViewModel>(slugValidation.Left);
                        }

                        publication.Slug = updatedPublication.Slug;

                        if (publication.Live
                            && _context.PublicationRedirects.All(pr =>
                                !(pr.PublicationId == publicationId && pr.Slug == originalSlug))) // don't create duplicate redirect
                        {
                            var publicationRedirect = new PublicationRedirect
                            {
                                Slug = originalSlug,
                                Publication = publication,
                                PublicationId = publication.Id,
                            };
                            _context.PublicationRedirects.Add(publicationRedirect);
                        }

                        // If there is an existing redirects for the new slug, they're redundant. Remove them
                        var redundantRedirects = await _context.PublicationRedirects
                            .Where(pr => pr.Slug == updatedPublication.Slug)
                            .ToListAsync();
                        if (redundantRedirects.Count > 0)
                        {
                            _context.PublicationRedirects.RemoveRange(redundantRedirects);
                        }
                    }

                    publication.Title = updatedPublication.Title;
                    publication.Summary = updatedPublication.Summary;
                    publication.TopicId = updatedPublication.TopicId;
                    publication.Updated = DateTime.UtcNow;
                    publication.SupersededById = updatedPublication.SupersededById;

                    _context.Publications.Update(publication);

                    await _context.SaveChangesAsync();

                    if (titleChanged || slugChanged)
                    {
                        await _methodologyService.PublicationTitleOrSlugChanged(publicationId,
                            originalSlug,
                            publication.Title,
                            publication.Slug);
                    }

                    if (publication.Live)
                    {
                        await _methodologyCacheService.UpdateSummariesTree();
                        await _publicationCacheService.UpdatePublicationTree();
                        await _publicationCacheService.UpdatePublication(publication.Slug);

                        if (slugChanged)
                        {
                            await _publicationCacheService.RemovePublication(originalSlug);
                            await _redirectsCacheService.UpdateRedirects();
                        }

                        await UpdateCachedSupersededPublications(publication);
                    }

                    return await GetPublication(publication.Id);
                });
        }

        private async Task UpdateCachedSupersededPublications(Publication publication)
        {
            // NOTE: When a publication is updated, any publication that is superseded by it can be affected, so
            // update any superseded publications that are cached
            var supersededPublications = await _context.Publications
                .Where(p => p.SupersededById == publication.Id)
                .ToListAsync();

            await supersededPublications
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(p => _publicationCacheService.UpdatePublication(p.Slug));
        }

        private async Task<Either<ActionResult, Unit>> ValidateSelectedTopic(
            Guid? topicId)
        {
            var topic = await _context.Topics.FindAsync(topicId);

            if (topic == null)
            {
                return ValidationActionResult(TopicDoesNotExist);
            }

            return await _userService.CheckCanCreatePublicationForTopic(topic)
                .OnSuccess(_ => Unit.Instance);
        }

        public async Task<Either<ActionResult, PublicationViewModel>> GetPublication(
            Guid publicationId, bool includePermissions = false)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId, HydratePublication)
                .OnSuccess(_userService.CheckCanViewPublication)
                .OnSuccess(publication => GeneratePublicationViewModel(publication, includePermissions));
        }

        public async Task<Either<ActionResult, ExternalMethodologyViewModel>> GetExternalMethodology(Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccessDo(_userService.CheckCanViewPublication)
                .OnSuccess(publication => publication.ExternalMethodology != null
                    ? new ExternalMethodologyViewModel(publication.ExternalMethodology)
                    : NotFound<ExternalMethodologyViewModel>());
        }

        public async Task<Either<ActionResult, ExternalMethodologyViewModel>> UpdateExternalMethodology(
            Guid publicationId, ExternalMethodologySaveRequest updatedExternalMethodology)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccessDo(_userService.CheckCanManageExternalMethodologyForPublication)
                .OnSuccess(async publication =>
                {
                    _context.Update(publication);
                    publication.ExternalMethodology ??= new ExternalMethodology();
                    publication.ExternalMethodology.Title = updatedExternalMethodology.Title;
                    publication.ExternalMethodology.Url = updatedExternalMethodology.Url;
                    await _context.SaveChangesAsync();

                    // Update publication cache because ExternalMethodology is in Content.Services.ViewModels.PublicationViewModel
                    await _publicationCacheService.UpdatePublication(publication.Slug);

                    return new ExternalMethodologyViewModel(publication.ExternalMethodology);
                });
        }

        public async Task<Either<ActionResult, Unit>> RemoveExternalMethodology(
            Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccessDo(_userService.CheckCanManageExternalMethodologyForPublication)
                .OnSuccess(async publication =>
                {
                    _context.Update(publication);
                    publication.ExternalMethodology = null;
                    await _context.SaveChangesAsync();

                    // Clear cache because ExternalMethodology is in Content.Services.ViewModels.PublicationViewModel
                    await _publicationCacheService.UpdatePublication(publication.Slug);

                    return Unit.Instance;
                });
        }

        public async Task<Either<ActionResult, ContactViewModel>> GetContact(Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId, query =>
                    query.Include(p => p.Contact))
                .OnSuccessDo(_userService.CheckCanViewPublication)
                .OnSuccess(publication => _mapper.Map<ContactViewModel>(publication.Contact));
        }

        public async Task<Either<ActionResult, ContactViewModel>> UpdateContact(Guid publicationId, ContactSaveRequest updatedContact)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId, query =>
                    query.Include(p => p.Contact))
                .OnSuccessDo(_userService.CheckCanUpdateContact)
                .OnSuccess(async publication =>
                {
                    // Replace existing contact that is shared with another publication with a new
                    // contact, as we want each publication to have its own contact.
                    if (_context.Publications
                        .Any(p => p.ContactId == publication.ContactId && p.Id != publication.Id))
                    {
                        publication.Contact = new Contact();
                    }

                    publication.Contact.ContactName = updatedContact.ContactName;
                    publication.Contact.ContactTelNo = string.IsNullOrWhiteSpace(updatedContact.ContactTelNo)
                        ? null
                        : updatedContact.ContactTelNo;
                    publication.Contact.TeamName = updatedContact.TeamName;
                    publication.Contact.TeamEmail = updatedContact.TeamEmail;
                    await _context.SaveChangesAsync();

                    // Clear cache because Contact is in Content.Services.ViewModels.PublicationViewModel
                    await _publicationCacheService.UpdatePublication(publication.Slug);

                    return _mapper.Map<ContactViewModel>(publication.Contact);
                });
        }

        public async Task<Either<ActionResult, PaginatedListViewModel<ReleaseSummaryViewModel>>>
            ListLatestReleaseVersionsPaginated(
                Guid publicationId,
                int page = 1,
                int pageSize = 5,
                bool? live = null,
                bool includePermissions = false)
        {
            return await ListLatestReleaseVersions(publicationId, live, includePermissions)
                .OnSuccess(
                    releases =>
                        // This is not ideal - we should paginate results in the database, however,
                        // this is not possible as we need to iterate over all releases to get the
                        // latest/active versions of releases. Ideally, we should be able to
                        // pagination entirely in the database, but this requires re-modelling of releases.
                        // TODO: EES-3663 Use database pagination when ReleaseVersions are introduced
                        PaginatedListViewModel<ReleaseSummaryViewModel>.Paginate(releases, page, pageSize)
                );
        }

        public async Task<Either<ActionResult, List<ReleaseSummaryViewModel>>> ListLatestReleaseVersions(
            Guid publicationId,
            bool? live = null,
            bool includePermissions = false)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccess(_userService.CheckCanViewPublication)
                .OnSuccess(async () =>
                {
                    // Note the 'live' filter is applied after the latest release versions are retrieved.
                    // A published release with a current draft version is deliberately not returned when 'live' is true.
                    var releaseVersions = (await _releaseVersionRepository.ListLatestReleaseVersions(publicationId))
                        .Where(rv => live == null || rv.Live == live)
                        .ToList();

                    return await releaseVersions
                        .ToAsyncEnumerable()
                        .SelectAwait(async rv => await HydrateReleaseListItemViewModel(rv, includePermissions))
                        .ToListAsync();
                });
        }

        public async Task<Either<ActionResult, List<ReleaseSeriesItemViewModel>>> GetReleaseSeriesView(Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccess(publication => _userService.CheckCanViewPublication(publication))
                .OnSuccess(async publication =>
                {
                    var result = new List<ReleaseSeriesItemViewModel>();
                    foreach (var seriesItem in publication.ReleaseSeriesView)
                    {
                        if (seriesItem.ReleaseId != null)
                        {
                            // @MarkFix on public, we'll want to filter out non-live release parents
                            var latestParentVersion = await _releaseVersionRepository
                                .GetLatestReleaseVersionForParent(
                                    publication.Id, seriesItem.ReleaseId.Value);

                            if (latestParentVersion == null)
                            {
                                throw new InvalidDataException("ReleaseSeriesItem with ReleaseParentId set should have an associated LatestReleaseVersion"); // @MarkFix improve message
                            }

                            result.Add(new ReleaseSeriesItemViewModel
                                {
                                    Id = seriesItem.Id,
                                    IsLegacyLink = false,
                                    Description = latestParentVersion.Title,
                                    ReleaseParentId = latestParentVersion.ReleaseId,
                                    PublicationSlug = publication.Slug,
                                    ReleaseSlug = latestParentVersion.Slug,
                                });

                        }
                        else // is legacy link
                        {

                            // @MarkFix check all prod legacy link descriptions/urls are not null/empty
                            if (seriesItem.LegacyLinkDescription == null)
                            {
                                throw new InvalidDataException(
                                    "If ReleaseSeriesItem's ReleaseParentId is null, LegacyLinkDescription should be set"); // @MarkFix improve message?
                            }

                            if (seriesItem.LegacyLinkUrl == null)
                            {
                                throw new InvalidDataException(
                                    "If ReleaseSeriesItem's ReleaseParentId is null, LegacyLinkUrl should be set"); // @MarkFix improve message?
                            }

                            result.Add(new ReleaseSeriesItemViewModel
                            {
                                Id = seriesItem.Id,
                                IsLegacyLink = true,
                                Description = seriesItem.LegacyLinkDescription,
                                LegacyLinkUrl = seriesItem.LegacyLinkUrl,
                            });
                        }
                    }

                    return result;
                });
        }

        // @MarkFix this needs tests
        public async Task<Either<ActionResult, List<ReleaseSeriesItem>>> AddReleaseSeriesLegacyLink( // @MarkFix should return view models?
            Guid publicationId,
            ReleaseSeriesLegacyLinkAddRequest newLegacyLink)
        {
            return await _context.Publications
                .FirstOrNotFoundAsync(p => p.Id == publicationId)
                .OnSuccess(_userService.CheckCanManageLegacyReleases)
                .OnSuccess(async publication =>
                {
                    publication.ReleaseSeriesView.Add(new ReleaseSeriesItem
                    {
                        Id = Guid.NewGuid(),
                        ReleaseId = null,
                        LegacyLinkDescription = newLegacyLink.Description,
                        LegacyLinkUrl = newLegacyLink.Url,
                    });

                    _context.Publications.Update(publication);
                    await _context.SaveChangesAsync();

                    return publication.ReleaseSeriesView;
                });
        }

        public async Task<Either<ActionResult, List<ReleaseSeriesItemUpdateRequest>>> UpdateReleaseSeries(
            Guid publicationId,
            List<ReleaseSeriesItemUpdateRequest> updatedReleaseSeriesItems)
        {
            return await _context.Publications
                .FirstOrNotFoundAsync(p => p.Id == publicationId)
                .OnSuccess(_userService.CheckCanManageLegacyReleases)
                .OnSuccess(async publication =>
                {
                    // @MarkFix check updatedReleaseSeriesItems contains sole copy of each releaseParent with correct id
                    // @MarkFix check each item has nothing else is set when ReleaseParentId is set

                    var newReleaseSeries = updatedReleaseSeriesItems
                        .Select(request => new ReleaseSeriesItem
                        {
                            Id = request.Id,
                            ReleaseId = request.ReleaseParentId,
                            LegacyLinkDescription = request.LegacyLinkDescription,
                            LegacyLinkUrl = request.LegacyLinkUrl,
                        }).ToList();

                    publication.ReleaseSeriesView = newReleaseSeries;
                    _context.Publications.Update(publication);

                    await _context.SaveChangesAsync();

                    await _publicationCacheService.UpdatePublication(publication.Slug);

                    return updatedReleaseSeriesItems; // @MarkFix do we want to return ReleaseSeriesItemViewModels here?
                });
        }

        private async Task<Either<ActionResult, Unit>> ValidatePublicationSlug(
            string newSlug, Guid? publicationId = null)
        {
            if (await _context.Publications.AsQueryable()
                    .AnyAsync(publication =>
                        publication.Id != publicationId
                        && publication.Slug == newSlug))
            {
                return ValidationActionResult(PublicationSlugNotUnique);
            }

            var hasRedirect = await _context.PublicationRedirects
                .AnyAsync(pr =>
                    pr.PublicationId != publicationId // If publication previously used this slug, can change it back
                    && pr.Slug == newSlug);

            if (hasRedirect)
            {
                return ValidationActionResult(PublicationSlugUsedByRedirect);
            }

            if (publicationId.HasValue &&
                _context.PublicationMethodologies.Any(pm =>
                    pm.Publication.Id == publicationId
                    && pm.Owner)
                // Strictly, we should also check whether the owned methodology inherits the publication slug - we don't
                // need to validate the new slug against methodologies if it isn't changing the methodology slug - but
                // this check is expensive and an unlikely edge case, so doesn't seem worth it.
                )
            {
                var methodologySlugValidation = await _methodologyService
                    .ValidateMethodologySlug(newSlug);
                if (methodologySlugValidation.IsLeft)
                {
                    return methodologySlugValidation.Left;
                }
            }

            return Unit.Instance;
        }

        public static IQueryable<Publication> HydratePublication(IQueryable<Publication> values)
        {
            return values
                .Include(p => p.Topic)
                .ThenInclude(topic => topic.Theme);
        }

        private async Task<PublicationViewModel> GeneratePublicationViewModel(Publication publication,
            bool includePermissions = false)
        {
            var publicationViewModel = _mapper.Map<PublicationViewModel>(publication);

            publicationViewModel.IsSuperseded = await _publicationRepository.IsSuperseded(publication.Id);

            if (includePermissions)
            {
                publicationViewModel.Permissions =
                    await PermissionsUtils.GetPublicationPermissions(_userService, publication);
            }

            return publicationViewModel;
        }

        private async Task<PublicationCreateViewModel> GeneratePublicationCreateViewModel(Publication publication)
        {
            var publicationCreateViewModel = _mapper.Map<PublicationCreateViewModel>(publication);

            publicationCreateViewModel.IsSuperseded = await _publicationRepository.IsSuperseded(publication.Id);

            return publicationCreateViewModel;
        }

        private async Task<ReleaseSummaryViewModel> HydrateReleaseListItemViewModel(ReleaseVersion releaseVersion,
            bool includePermissions)
        {
            var viewModel = _mapper.Map<ReleaseSummaryViewModel>(releaseVersion);

            //viewModel.Order = publication.ReleaseSeriesView.Find(ro => ro.ReleaseId == release.Id)?.Order ?? 0; // @MarkFix
            //viewModel.IsDraft = !release.Live;

            if (includePermissions)
            {
                viewModel.Permissions = await PermissionsUtils.GetReleasePermissions(_userService, releaseVersion);
            }

            return viewModel;
        }
    }
}
