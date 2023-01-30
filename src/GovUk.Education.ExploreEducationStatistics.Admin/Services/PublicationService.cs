#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using IPublicationRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationRepository;

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

        public PublicationService(
            ContentDbContext context,
            IMapper mapper,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            IPublicationRepository publicationRepository,
            IMethodologyVersionRepository methodologyVersionRepository,
            IPublicationCacheService publicationCacheService,
            IMethodologyCacheService methodologyCacheService)
        {
            _context = context;
            _mapper = mapper;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _publicationRepository = publicationRepository;
            _methodologyVersionRepository = methodologyVersionRepository;
            _publicationCacheService = publicationCacheService;
            _methodologyCacheService = methodologyCacheService;
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
                    publication.Updated = DateTime.UtcNow;
                    publication.SupersededById = updatedPublication.SupersededById;

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
                        await _methodologyCacheService.UpdateSummariesTree();
                        await _publicationCacheService.UpdatePublicationTree();
                        await _publicationCacheService.UpdatePublication(publication.Slug);

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

        public async Task<Either<ActionResult, ContactViewModel>> UpdateContact(Guid publicationId, Contact updatedContact)
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
                    publication.Contact.ContactTelNo = updatedContact.ContactTelNo;
                    publication.Contact.TeamName = updatedContact.TeamName;
                    publication.Contact.TeamEmail = updatedContact.TeamEmail;
                    await _context.SaveChangesAsync();

                    // Clear cache because Contact is in Content.Services.ViewModels.PublicationViewModel
                    await _publicationCacheService.UpdatePublication(publication.Slug);

                    return _mapper.Map<ContactViewModel>(updatedContact);
                });
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
                .OnSuccess(_userService.CheckCanManageLegacyReleases)
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

        private async Task<ReleaseSummaryViewModel> HydrateReleaseListItemViewModel(Release release, bool includePermissions)
        {
            var viewModel = _mapper.Map<ReleaseSummaryViewModel>(release);

            if (includePermissions)
            {
                viewModel.Permissions = await PermissionsUtils.GetReleasePermissions(_userService, release);
            }

            return viewModel;
        }
    }
}
