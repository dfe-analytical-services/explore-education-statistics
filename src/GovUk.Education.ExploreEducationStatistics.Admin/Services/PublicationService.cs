using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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

        public async Task<Either<ActionResult, List<MyPublicationViewModel>>> GetMyPublicationsAndReleasesByTopicAsync(
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

        public async Task<Either<ActionResult, PublicationViewModel>> CreatePublicationAsync(
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

                    var saved = _context.Publications.Add(new Publication
                    {
                        Id = Guid.NewGuid(),
                        ContactId = publication.ContactId,
                        Title = publication.Title,
                        TopicId = publication.TopicId,
                        MethodologyId = publication.MethodologyId,
                        Slug = publication.Slug,
                        ExternalMethodology = publication.ExternalMethodology
                    });

                    _context.SaveChanges();
                    return await GetViewModelAsync(saved.Entity.Id);
                });
        }

        public async Task<Either<ActionResult, PublicationViewModel>> GetViewModelAsync(Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId, HydratePublicationForPublicationViewModel)
                .OnSuccess(_userService.CheckCanViewPublication)
                .OnSuccess(publication => _mapper.Map<PublicationViewModel>(publication));
        }

        public async Task<Either<ActionResult, bool>> UpdatePublicationMethodologyAsync(Guid publicationId,
            UpdatePublicationMethodologyViewModel methodology)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccess(_ => ValidateMethodologyCanBeAssigned(methodology))
                .OnSuccess(async _ =>
                {
                    var publication = await _context.Publications.Include(p => p.Methodology)
                        .FirstOrDefaultAsync(p => p.Id == publicationId);

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

        public static IQueryable<Publication> HydratePublicationForPublicationViewModel(IQueryable<Publication> values)
        {
            return values.Include(p => p.Contact)
                .Include(p => p.Releases)
                .ThenInclude(r => r.Type)
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
                if (methodology.Status == "Draft")
                {
                    return ValidationActionResult(MethodologyMustBeApprovedOrPublished);
                }
            }
            else if (model.ExternalMethodology != null && model.MethodologyId == null)
            {
                // TODO: EES-1287 External methodology must be external link to the service
            }
            else
            {
                return ValidationActionResult(CannotSpecifyMethodologyAndExternalMethodology);
            }

            return true;
        }
    }
}