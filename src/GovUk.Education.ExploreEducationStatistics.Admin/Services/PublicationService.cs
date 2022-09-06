﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using LegacyReleaseViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.LegacyReleaseViewModel;
using MethodologyVersionSummaryViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.MethodologyVersionSummaryViewModel;
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
        private readonly IMethodologyVersionRepository _methodologyVersionRepository;
        private readonly IPublicationCacheService _publicationCacheService;
        private readonly IMethodologyCacheService _methodologyCacheService;
        private readonly IThemeCacheService _themeCacheService;

        public PublicationService(
            ContentDbContext context,
            IMapper mapper,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            IPublicationRepository publicationRepository,
            IMethodologyVersionRepository methodologyVersionRepository,
            IPublicationCacheService publicationCacheService, 
            IMethodologyCacheService methodologyCacheService, 
            IThemeCacheService themeCacheService)
        {
            _context = context;
            _mapper = mapper;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _publicationRepository = publicationRepository;
            _methodologyVersionRepository = methodologyVersionRepository;
            _publicationCacheService = publicationCacheService;
            _methodologyCacheService = methodologyCacheService;
            _themeCacheService = themeCacheService;
        }

        public async Task<Either<ActionResult, List<MyPublicationViewModel>>> GetMyPublicationsAndReleasesByTopic(
            Guid topicId)
        {
            return await _userService
                .CheckCanAccessSystem()
                .OnSuccess(_ => _userService.CheckCanViewAllReleases()
                    .OnSuccess(() => _publicationRepository.GetAllPublicationsForTopic(topicId))
                    .OrElse(() =>
                        _publicationRepository.GetPublicationsForTopicRelatedToUser(topicId, _userService.GetUserId()))
                )
                .OnSuccess(async publicationViewModels =>
                {
                    return (await HydrateMyPublicationsViewModels(publicationViewModels))
                        .OrderBy(publicationViewModel => publicationViewModel.Title)
                        .ToList();
                });
        }

        public async Task<Either<ActionResult, MyPublicationViewModel>> GetMyPublication(Guid publicationId)
        {
            var userId = _userService.GetUserId();

            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccess(_userService.CheckCanAccessSystem)
                .OnSuccess(_ => _userService.CheckCanViewAllReleases()
                    .OnSuccess(_ => _publicationRepository.GetPublicationWithAllReleases(publicationId))
                    .OrElse(async () =>
                    {
                        var publication = _context.Find<Publication>(publicationId);
                        return await _userService.CheckCanViewPublication(publication!)
                            .OnSuccess(_ => _publicationRepository.GetPublicationForUser(publicationId, userId));
                    })
                ).OnSuccess(HydrateMyPublicationViewModel);
        }

        public async Task<Either<ActionResult, List<PublicationSummaryViewModel>>> ListPublicationSummaries()
        {
            return await _userService
                .CheckCanManageAllUsers()
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

        public async Task<Either<ActionResult, PublicationViewModel>> CreatePublication(
            PublicationSaveViewModel publication)
        {
            return await ValidateSelectedTopic(publication.TopicId)
                .OnSuccess(async _ =>
                {
                    if (_context.Publications.Any(p => p.Slug == publication.Slug))
                    {
                        return ValidationActionResult(SlugNotUnique);
                    }

                    var contact = await _context.Contacts.AddAsync(new Contact
                    {
                        ContactName = publication.Contact.ContactName,
                        ContactTelNo = publication.Contact.ContactTelNo,
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
                        ExternalMethodology = publication.ExternalMethodology
                    });

                    await _context.SaveChangesAsync();

                    return await GetPublication(saved.Entity.Id);
                });
        }

        public async Task<Either<ActionResult, PublicationViewModel>> UpdatePublication(
            Guid publicationId,
            PublicationSaveViewModel updatedPublication)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccess(_userService.CheckCanUpdatePublication)
                .OnSuccessDo(async publication =>
                {
                    if (publication.Title != updatedPublication.Title)
                    {
                        return await _userService.CheckCanUpdatePublicationTitle();
                    }

                    return Unit.Instance;
                })
                .OnSuccessDo(async publication =>
                {
                    if (publication.SupersededById != updatedPublication.SupersededById)
                    {
                        return await _userService.CheckCanUpdatePublicationSupersededBy();
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
                .OnSuccess(async publication =>
                {
                    var originalTitle = publication.Title;
                    var originalSlug = publication.Slug;

                    if (!publication.Live)
                    {
                        var slugValidation =
                            await ValidatePublicationSlugUniqueForUpdate(publication.Id, updatedPublication.Slug);

                        if (slugValidation.IsLeft)
                        {
                            return new Either<ActionResult, PublicationViewModel>(slugValidation.Left);
                        }

                        publication.Slug = updatedPublication.Slug;
                    }

                    publication.Title = updatedPublication.Title;
                    publication.Summary = updatedPublication.Summary;
                    publication.TopicId = updatedPublication.TopicId;
                    publication.ExternalMethodology = updatedPublication.ExternalMethodology;
                    publication.Updated = DateTime.UtcNow;
                    publication.SupersededById = updatedPublication.SupersededById;

                    // Add new contact if it doesn't exist, otherwise replace existing
                    // contact that is shared with another publication with a new
                    // contact, as we want each publication to have its own contact.
                    if (publication.Contact == null ||
                        _context.Publications
                            .Any(p => p.ContactId == publication.ContactId && p.Id != publication.Id))
                    {
                        publication.Contact = new Contact();
                    }

                    publication.Contact.ContactName = updatedPublication.Contact.ContactName;
                    publication.Contact.ContactTelNo = updatedPublication.Contact.ContactTelNo;
                    publication.Contact.TeamName = updatedPublication.Contact.TeamName;
                    publication.Contact.TeamEmail = updatedPublication.Contact.TeamEmail;

                    _context.Publications.Update(publication);

                    await _context.SaveChangesAsync();

                    if (originalTitle != publication.Title)
                    {
                        await _methodologyVersionRepository.PublicationTitleChanged(publicationId,
                            originalSlug,
                            publication.Title,
                            publication.Slug);
                    }

                    if (publication.Live)
                    {
                        publication.Published = DateTime.UtcNow;
                        await _context.SaveChangesAsync();

                        await _methodologyCacheService.UpdateSummariesTree();
                        await _themeCacheService.UpdatePublicationTree();
                        await _publicationCacheService.UpdatePublication(publication.Slug);

                        await UpdateCachedSupersededPublications(publication);
                    }

                    return await GetPublication(publication.Id);
                });
        }

        private Task UpdateCachedSupersededPublications(Publication publication)
        {
            // NOTE: When a publication is updated, any publication that is superseded by it can be affected, so
            // update any superseded publications that are cached
            return _context.Publications
                .Where(p => p.SupersededById == publication.Id)
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

        public async Task<Either<ActionResult, PublicationViewModel>> GetPublication(Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId, HydratePublicationForPublicationViewModel)
                .OnSuccess(_userService.CheckCanViewPublication)
                .OnSuccess(publication => _mapper.Map<PublicationViewModel>(publication));
        }

        public async Task<Either<ActionResult, PaginatedListViewModel<ReleaseSummaryViewModel>>>
            ListActiveReleasesPaginated(
                Guid publicationId,
                int page = 1,
                int pageSize = 5,
                bool? live = null,
                bool includePermissions = false)
        {
            return await ListActiveReleases(publicationId, live, includePermissions)
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

        public async Task<Either<ActionResult, List<ReleaseSummaryViewModel>>> ListActiveReleases(
            Guid publicationId,
            bool? live = null,
            bool includePermissions = false)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId, query => query
                    .Include(p => p.Releases)
                    .ThenInclude(r => r.ReleaseStatuses))
                .OnSuccess(_userService.CheckCanViewPublication)
                .OnSuccess(async publication =>
                {
                    var activeReleases = publication.ListActiveReleases()
                        .Where(release => live == null || release.Live == live)
                        .OrderByDescending(r => r.Year)
                        .ThenByDescending(r => r.TimePeriodCoverage);

                    return await activeReleases
                        .ToAsyncEnumerable()
                        .SelectAwait(async r => await HydrateReleaseListItemViewModel(r, includePermissions))
                        .ToListAsync();
                });
        }

        public async Task<Either<ActionResult, List<LegacyReleaseViewModel>>> PartialUpdateLegacyReleases(
            Guid publicationId,
            List<LegacyReleasePartialUpdateViewModel> updatedLegacyReleases)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(
                    publicationId,
                    publication => publication.Include(p => p.LegacyReleases)
                )
                .OnSuccess(_userService.CheckCanUpdatePublication)
                .OnSuccess(async publication =>
                {
                    publication.LegacyReleases.ForEach(legacyRelease =>
                    {
                        var updateLegacyRelease = updatedLegacyReleases
                            .Find(release => release.Id == legacyRelease.Id);

                        if (updateLegacyRelease == null)
                        {
                            return;
                        }

                        legacyRelease.Description = updateLegacyRelease.Description ?? legacyRelease.Description;
                        legacyRelease.Url = updateLegacyRelease.Url ?? legacyRelease.Url;

                        // Don't shift other orders around as its unreliable when doing
                        // a bulk update like this. It's easier to let the consumer
                        // decide what all the orders should be.
                        legacyRelease.Order = updateLegacyRelease.Order ?? legacyRelease.Order;

                        _context.Update(legacyRelease);
                    });

                    _context.Update(publication);
                    await _context.SaveChangesAsync();

                    await _publicationCacheService.UpdatePublication(publication.Slug);

                    return _mapper.Map<List<LegacyReleaseViewModel>>(
                        publication.LegacyReleases.OrderByDescending(release => release.Order)
                    );
                });
        }

        private async Task<Either<ActionResult, Unit>> ValidatePublicationSlugUniqueForUpdate(Guid id, string slug)
        {
            if (await _context.Publications.AsQueryable()
                    .AnyAsync(publication => publication.Slug == slug && publication.Id != id))
            {
                return ValidationActionResult(SlugNotUnique);
            }

            return Unit.Instance;
        }

        public static IQueryable<Publication> HydratePublicationForPublicationViewModel(IQueryable<Publication> values)
        {
            return values
                .AsSplitQuery()
                .Include(p => p.Contact)
                .Include(p => p.Releases)
                .ThenInclude(r => r.ReleaseStatuses)
                .Include(p => p.Topic)
                .Include(p => p.Methodologies)
                .ThenInclude(p => p.Methodology)
                .ThenInclude(p => p.Versions);
        }

        private async Task<List<MyPublicationViewModel>> HydrateMyPublicationsViewModels(List<Publication> publications)
        {
            return await publications
                .ToAsyncEnumerable()
                .SelectAwait(async publication => await HydrateMyPublicationViewModel(publication))
                .ToListAsync();
        }

        private async Task<MyPublicationViewModel> HydrateMyPublicationViewModel(Publication publication)
        {
            var publicationViewModel = _mapper.Map<MyPublicationViewModel>(publication);

            publicationViewModel.IsSuperseded = _publicationRepository.IsSuperseded(publication);

            await publicationViewModel.Releases
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async releaseViewModel =>
            {
                var release = publication.Releases.Single(release => release.Id == releaseViewModel.Id);
                releaseViewModel.Permissions = await PermissionsUtils.GetReleasePermissions(_userService, release);
            });

            publicationViewModel.Methodologies = (await HydrateMethodologyVersionViewModels(publication))
                .OrderBy(viewModel => viewModel.Title)
                .ToList();

            return publicationViewModel;
        }

        private async Task<ReleaseSummaryViewModel> HydrateReleaseListItemViewModel(Release release, bool includePermissions)
        {
            var viewModel = _mapper.Map<ReleaseSummaryViewModel>(release);

            if (includePermissions)
            {
                viewModel.Permissions = await PermissionsUtils.GetReleasePermissions(_userService, release);
            }

            return viewModel;
        }

        private async Task<List<MethodologyVersionSummaryViewModel>> HydrateMethodologyVersionViewModels(
            Publication publication)
        {
            return await publication.Methodologies
                .ToAsyncEnumerable()
                .SelectAwait(async publicationMethodology =>
                {
                    var latestVersion = publicationMethodology.Methodology.LatestVersion();
                    var permissions = await PermissionsUtils.GetMethodologyVersionPermissions(_userService,
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
                        InternalReleaseNote = latestVersion.InternalReleaseNote,
                        MethodologyId = latestVersion.MethodologyId,
                        PreviousVersionId = latestVersion.PreviousVersionId,
                        Permissions = permissions
                    };
                })
                .ToListAsync();
        }
    }
}
