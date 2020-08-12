using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
#nullable enable annotations
    public class PublicationService : IPublicationService
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IPublicationRepository _publicationRepository;
        private readonly IPublishingService _publishingService;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;

        public PublicationService(
            ContentDbContext context,
            IMapper mapper,
            IUserService userService,
            IPublicationRepository publicationRepository,
            IPublishingService publishingService,
            IPersistenceHelper<ContentDbContext> persistenceHelper)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
            _publicationRepository = publicationRepository;
            _publishingService = publishingService;
            _persistenceHelper = persistenceHelper;
        }

        public async Task<Either<ActionResult, List<MyPublicationViewModel>>> GetMyPublicationsAndReleasesByTopic(
            Guid topicId)
        {
            return await _userService
                .CheckCanAccessSystem()
                .OnSuccess(_ =>
                {
                    return _userService
                        .CheckCanViewAllReleases()
                        .OnSuccess(() =>
                            _publicationRepository.GetAllPublicationsForTopicAsync(topicId))
                        .OrElse(() =>
                            _publicationRepository.GetPublicationsForTopicRelatedToUserAsync(topicId,
                                _userService.GetUserId()));
                });
        }

        public async Task<Either<ActionResult, PublicationViewModel>> CreatePublication(
            SavePublicationViewModel publication)
        {
            return await ValidateSelectedTopic(publication.TopicId)
                .OnSuccess(_ => ValidateSelectedMethodology(
                    publication.MethodologyId,
                    publication.ExternalMethodology
                ))
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
                        TeamEmail = publication.Contact.TeamEmail,
                    });

                    var saved = await _context.Publications.AddAsync(new Publication
                    {
                        Contact = contact.Entity,
                        Title = publication.Title,
                        TopicId = publication.TopicId,
                        MethodologyId = publication.MethodologyId,
                        Slug = publication.Slug,
                        ExternalMethodology = publication.ExternalMethodology
                    });

                    await _context.SaveChangesAsync();

                    return await GetViewModel(saved.Entity.Id);
                });
        }

        public async Task<Either<ActionResult, PublicationViewModel>> UpdatePublication(
            Guid publicationId,
            SavePublicationViewModel updatedPublication)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccess(_userService.CheckCanUpdatePublication)
                .OnSuccessDo(_ => ValidateSelectedTopic(updatedPublication.TopicId))
                .OnSuccessDo(_ => ValidateSelectedMethodology(
                    updatedPublication.MethodologyId,
                    updatedPublication.ExternalMethodology
                ))
                .OnSuccess(async publication =>
                {
                    if (_context.Publications
                        .Any(p => p.Slug == updatedPublication.Slug && p.Id != publication.Id))
                    {
                        return ValidationActionResult(SlugNotUnique);
                    }

                    var oldSlug = publication.Slug;
                    
                    publication.Title = updatedPublication.Title;
                    publication.TopicId = updatedPublication.TopicId;
                    publication.MethodologyId = updatedPublication.MethodologyId;
                    publication.Slug = updatedPublication.Slug;
                    publication.ExternalMethodology = updatedPublication.ExternalMethodology;

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

                    if (publication.Live)
                    {
                        await _publishingService.PublicationChanged(publication.Id, oldSlug);
                    }

                    return await GetViewModel(publication.Id);
                });
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

        private async Task<Either<ActionResult, Unit>> ValidateSelectedMethodology(
            Guid? methodologyId,
            ExternalMethodology? externalMethodology)
        {
            if (methodologyId != null && externalMethodology != null)
            {
                return ValidationActionResult(CannotSpecifyMethodologyAndExternalMethodology);
            }

            if (methodologyId != null)
            {
                var methodology = await _context.Methodologies.FindAsync(methodologyId);

                if (methodology == null)
                {
                    return ValidationActionResult(MethodologyDoesNotExist);
                }

                // TODO: this will want updating when methodology status is fully implemented
                if (methodology.Status != MethodologyStatus.Approved)
                {
                    return ValidationActionResult(MethodologyMustBeApprovedOrPublished);
                }
            }

            return Unit.Instance;
        }

        public async Task<Either<ActionResult, PublicationViewModel>> GetViewModel(Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId, HydratePublicationForPublicationViewModel)
                .OnSuccess(_userService.CheckCanViewPublication)
                .OnSuccess(publication => _mapper.Map<PublicationViewModel>(publication));
        }

        public async Task<Either<ActionResult, List<LegacyReleaseViewModel>>> PartialUpdateLegacyReleases(
            Guid publicationId,
            List<PartialUpdateLegacyReleaseViewModel> updatedLegacyReleases)
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

                    return _mapper.Map<List<LegacyReleaseViewModel>>(
                        publication.LegacyReleases.OrderByDescending(release => release.Order)
                    );
                });
        }

        public static IQueryable<Publication> HydratePublicationForPublicationViewModel(IQueryable<Publication> values)
        {
            return values.Include(p => p.Contact)
                .Include(p => p.Releases)
                .ThenInclude(r => r.Type)
                .Include(p => p.LegacyReleases)
                .Include(p => p.Methodology)
                .Include(p => p.Topic);
        }
    }
}