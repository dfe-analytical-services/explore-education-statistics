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
    public class PublicationService : IPublicationService
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IPublicationRepository _publicationRepository;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;

        public PublicationService(ContentDbContext context, IMapper mapper, IUserService userService,
            IPublicationRepository publicationRepository, IPersistenceHelper<ContentDbContext> persistenceHelper)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
            _publicationRepository = publicationRepository;
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
            CreatePublicationViewModel publication)
        {
            return await _persistenceHelper
                .CheckEntityExists<Topic>(publication.TopicId)
                .OnSuccess(_userService.CheckCanCreatePublicationForTopic)
                .OnSuccess(async _ =>
                {
                    if (_context.Publications.Any(p => p.Slug == publication.Slug))
                    {
                        return ValidationActionResult<PublicationViewModel>(SlugNotUnique);
                    }

                    var saved = await _context.Publications.AddAsync(new Publication
                    {
                        ContactId = publication.ContactId,
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

        public async Task<Either<ActionResult, PublicationViewModel>> GetViewModel(Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId, HydratePublicationForPublicationViewModel)
                .OnSuccess(_userService.CheckCanViewPublication)
                .OnSuccess(publication => _mapper.Map<PublicationViewModel>(publication));
        }

        public async Task<Either<ActionResult, bool>> UpdatePublicationMethodology(Guid publicationId,
            UpdatePublicationMethodologyViewModel methodology)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccess(_ => ValidateMethodologyCanBeAssigned(methodology))
                .OnSuccess(async _ =>
                {
                    var publication = await _context.Publications.Include(p => p.ExternalMethodology).FirstOrDefaultAsync(p => p.Id == publicationId);

                    _context.Publications.Update(publication);
                    
                    if (methodology.MethodologyId != null)
                    {
                        publication.MethodologyId = methodology.MethodologyId;
                        publication.ExternalMethodology = null;
                    }
                    else
                    {
                        publication.ExternalMethodology = methodology.ExternalMethodology;
                        publication.MethodologyId = null;
                    }
                    _context.SaveChanges();

                    return true;
                });
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

        private async Task<Either<ActionResult, bool>> ValidateMethodologyCanBeAssigned(
            UpdatePublicationMethodologyViewModel model)
        {
            if (model.MethodologyId != null && model.ExternalMethodology == null)
            {
                var methodology = await _context.Methodologies.FirstOrDefaultAsync(p => p.Id == model.MethodologyId);

                if (methodology == null)
                {
                    return ValidationActionResult(MethodologyDoesNotExist);
                }

                // TODO: this will want updating when methodology status is fully implemented
                if (methodology.Status == MethodologyStatus.Draft)
                {
                    return ValidationActionResult(MethodologyMustBeApprovedOrPublished);
                }
            }
            else if (model.ExternalMethodology != null && model.MethodologyId == null)
            {
                // TODO: EES-1287 External methodology must be external link to the service
            }
            else if (model.ExternalMethodology == null && model.MethodologyId == null)
            {
                return ValidationActionResult(MethodologyOrExternalMethodologyLinkMustBeDefined);
            }
            else
            {
                return ValidationActionResult(CannotSpecifyMethodologyAndExternalMethodology);
            }

            return true;
        }
    }
}
