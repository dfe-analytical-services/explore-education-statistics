using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class MethodologyService : IMethodologyService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
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
                .OnSuccess(() => _methodologyRepository.CreateMethodologyForPublication(publicationId))
                .OnSuccess(_mapper.Map<MethodologySummaryViewModel>);
        }

        public async Task<Either<ActionResult, MethodologySummaryViewModel>> GetSummary(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Methodology>(id)
                .OnSuccess(_userService.CheckCanViewMethodology)
                .OnSuccess(_mapper.Map<MethodologySummaryViewModel>);
        }

        public async Task<Either<ActionResult, List<MethodologyPublicationsViewModel>>> ListWithPublications()
        {
            return await _userService
                .CheckCanViewAllMethodologies()
                .OnSuccess(async () =>
                {
                    return (await _context.PublicationMethodologies
                        .Include(pm => pm.Publication)
                        .ToList()
                        .GroupBy(pm => pm.MethodologyParentId)
                        .SelectAsync(async grouping =>
                    {
                        // TODO EES-2156 This intentionally only selects the first Methodology found
                        // It will need updating when Methodology amendments are introduced
                        // but it's likely the Methodology Dashboard will be removed before then
                        var methodology = await _context.Methodologies.FirstAsync(m => m.MethodologyParentId == grouping.Key);

                        return new MethodologyPublicationsViewModel
                        {
                            Id = methodology.Id,
                            Title = methodology.Title,
                            Status = methodology.Status.ToString(),
                            Publications = grouping.Select(publicationMethodology => new IdTitlePair
                            {
                                Id = publicationMethodology.Publication.Id,
                                Title = publicationMethodology.Publication.Title
                            }).ToList()
                        };
                    })).ToList();
                });
        }

        public async Task<Either<ActionResult, MethodologySummaryViewModel>> UpdateMethodology(Guid id,
            MethodologyUpdateRequest request)
        {
            return await _persistenceHelper.CheckEntityExists<Methodology>(id)
                .OnSuccess(methodology => CheckCanUpdateMethodologyStatus(methodology, request.Status))
                .OnSuccess(_userService.CheckCanUpdateMethodology)
                .OnSuccessDo(methodology => RemoveUnusedImages(methodology.Id))
                .OnSuccess(async methodology =>
                {
                    if (methodology.Live)
                    {
                        // Leave slug
                        return methodology;
                    }
                    var slug = SlugFromTitle(request.Title);
                    return (await ValidateMethodologySlugUniqueForUpdate(methodology.Id, slug)).Map(_ =>
                    {
                        methodology.Slug = slug;
                        return methodology;
                    });
                })
                .OnSuccess(async methodology =>
                {
                    methodology.InternalReleaseNote = request.LatestInternalReleaseNote ?? methodology.InternalReleaseNote;
                    methodology.Status = request.Status;
                    methodology.Title = request.Title;
                    methodology.Updated = DateTime.UtcNow;

                    _context.Methodologies.Update(methodology);
                    await _context.SaveChangesAsync();

                    if (methodology.Status == MethodologyStatus.Approved && methodology.Live)
                    {
                        await _publishingService.MethodologyChanged(methodology.Id);
                    }

                    return await GetSummary(id);
                });
        }

        private Task<Either<ActionResult, Methodology>> CheckCanUpdateMethodologyStatus(Methodology methodology,
            MethodologyStatus status)
        {
            if (methodology.Status == status)
            {
                // Status unchanged
                return Task.FromResult(new Either<ActionResult, Methodology>(methodology));
            }

            return status switch
            {
                MethodologyStatus.Draft => _userService.CheckCanMarkMethodologyAsDraft(methodology),
                MethodologyStatus.Approved => _userService.CheckCanApproveMethodology(methodology),
                _ => throw new ArgumentOutOfRangeException(nameof(status), "Unexpected status")
            };
        }

        private async Task<Either<ActionResult, Unit>> RemoveUnusedImages(Guid methodologyId)
        {
            return await _methodologyContentService.GetContentBlocks<HtmlBlock>(methodologyId)
                .OnSuccess(async contentBlocks =>
                {
                    var contentImageIds = contentBlocks.SelectMany(contentBlock =>
                            HtmlImageUtil.GetMethodologyImages(contentBlock.Body))
                        .Distinct();

                    var imageFiles = await _methodologyFileRepository.GetByFileType(methodologyId, Image);

                    var unusedImages = imageFiles
                        .Where(file => !contentImageIds.Contains(file.File.Id))
                        .Select(file => file.File.Id)
                        .ToList();

                    if (unusedImages.Any())
                    {
                        return await _methodologyImageService.Delete(methodologyId, unusedImages);
                    }

                    return Unit.Instance;
                });
        }

        private async Task<Either<ActionResult, Unit>> ValidateMethodologySlugUniqueForUpdate(Guid id, string slug)
        {
            if (await _context.Methodologies.AnyAsync(methodology => methodology.Slug == slug && methodology.Id != id))
            {
                return ValidationActionResult(SlugNotUnique);
            }

            return Unit.Instance;
        }
    }
}
