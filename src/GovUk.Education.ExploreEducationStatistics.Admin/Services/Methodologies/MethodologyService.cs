using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class MethodologyService : IMethodologyService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
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
            ICacheService cacheService,
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
            _cacheService = cacheService;
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
                .CheckEntityExists<Methodology>(id, HydrateMethodologyForMethodologySummaryViewModel)
                .OnSuccess(_userService.CheckCanViewMethodology)
                .OnSuccess(methodology =>
                {
                    var publicationLinks = methodology.MethodologyParent.Publications;
                    var owningPublication = BuildPublicationViewModel(publicationLinks.Single(pm => pm.Owner));
                    var otherPublications = publicationLinks.Where(pm => !pm.Owner)
                        .Select(BuildPublicationViewModel)
                        .OrderBy(model => model.Title)
                        .ToList();

                    var viewModel = _mapper.Map<MethodologySummaryViewModel>(methodology);
                    viewModel.Publication = owningPublication;
                    viewModel.OtherPublications = otherPublications;
                    return viewModel;
                });
        }

        public async Task<Either<ActionResult, MethodologySummaryViewModel>> UpdateMethodology(Guid id,
            MethodologyUpdateRequest request)
        {
            return await _persistenceHelper.CheckEntityExists<Methodology>(id, 
                    HydrateMethodologyForManageMethodologySummaryViewModel)
                .OnSuccess(methodology => CheckCanUpdateMethodologyStatus(methodology, request.Status))
                .OnSuccess(_userService.CheckCanUpdateMethodology)
                .OnSuccessDo(methodology => RemoveUnusedImages(methodology.Id))
                .OnSuccess(async methodology =>
                {
                    // TODO SOW4 EES-2166 EES-2200:
                    // In future it probably won't be necessary to do this,
                    // since it won't be possible to change the title of a methodology that's already publicly accessible.
                    // Prevent the slug from being changed on amendments instead.
                    if (await _methodologyRepository.IsPubliclyAccessible(methodology.Id))
                    {
                        // Leave slug
                        return methodology;
                    }
                    var slug = SlugFromTitle(request.Title);
                    return (await ValidateMethodologySlugUniqueForUpdate(methodology.Id, slug)).OnSuccess(_ =>
                    {
                        methodology.MethodologyParent.Slug = slug;
                        return methodology;
                    });
                })
                .OnSuccess(async methodology =>
                {
                    _context.Methodologies.Update(methodology);

                    methodology.InternalReleaseNote = request.LatestInternalReleaseNote ?? methodology.InternalReleaseNote;
                    methodology.PublishingStrategy = request.PublishingStrategy;
                    methodology.Status = request.Status;
                    methodology.AlternativeTitle = request.Title;
                    methodology.Updated = DateTime.UtcNow;

                    if (await _methodologyRepository.IsPubliclyAccessible(methodology.Id))
                    {
                        await _publishingService.PublishMethodologyFiles(methodology.Id);

                        // TODO SOW4 EES-2166 EES-2200
                        // Until it's possible to create an amendment we still allow un-approving and re-approving.
                        // This means the methodology may already have a published date that it was first published on,
                        // so don't overwrite it. Later this can be changed to:
                        // methodology.Published = DateTime.UtcNow;

                        methodology.Published ??= DateTime.UtcNow;

                        // Invalidate the 'All Methodologies' cache item
                        await _cacheService.DeleteItem(PublicContent, AllMethodologiesCacheKey.Instance);
                    }

                    await _context.SaveChangesAsync();

                    return await GetSummary(id);
                });
        }

        public Task<Either<ActionResult, Unit>> DeleteMethodology(Guid methodologyId)
        {
            return _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId, 
                    query => query.Include(m => m.MethodologyParent))
                .OnSuccess(_userService.CheckCanDeleteMethodology)
                .OnSuccessDo(UnlinkMethodologyFilesAndDeleteIfOrphaned)
                .OnSuccessDo(DeleteMethodologyVersion)
                .OnSuccessVoid(DeleteMethodologyParentIfOrphaned);
        }

        private async Task<Either<ActionResult, Unit>> UnlinkMethodologyFilesAndDeleteIfOrphaned(Methodology methodology)
        {
            var methodologyFileIds = await _context
                    .MethodologyFiles
                    .Where(f => f.MethodologyId == methodology.Id)
                    .Select(f => f.FileId)
                    .ToListAsync();

            if (methodologyFileIds.Count > 0)
            {
                return await _methodologyImageService.UnlinkAndDeleteIfOrphaned(methodology.Id, methodologyFileIds);
            }

            return Unit.Instance;
        }
        
        private async Task DeleteMethodologyVersion(Methodology methodology)
        {
            _context.Methodologies.Remove(methodology);
            await _context.SaveChangesAsync();
        }
        
        private async Task DeleteMethodologyParentIfOrphaned(Methodology methodology)
        {
            var methodologyParent = await _context
                .MethodologyParents
                .Include(p => p.Versions)
                .SingleAsync(p => p.Id == methodology.MethodologyParentId);
                    
            if (methodologyParent.Versions.Count == 0)
            {
                _context.MethodologyParents.Remove(methodologyParent);
                await _context.SaveChangesAsync();
            }
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
                        return await _methodologyImageService.UnlinkAndDeleteIfOrphaned(methodologyId, unusedImages);
                    }

                    return Unit.Instance;
                });
        }

        private static TitleAndIdViewModel BuildPublicationViewModel(PublicationMethodology publicationMethodology)
        {
            var publication = publicationMethodology.Publication;
            return new TitleAndIdViewModel(publication.Id, publication.Title);
        }

        private async Task<Either<ActionResult, Unit>> ValidateMethodologySlugUniqueForUpdate(Guid id, string slug)
        {
            var methodologyParentId = await _context
                .Methodologies
                .Where(m => m.Id == id)
                .Select(m => m.MethodologyParentId)
                .SingleAsync();
            
            if (await _context
                .MethodologyParents
                .AnyAsync(p => p.Slug == slug && p.Id != methodologyParentId))
            {
                return ValidationActionResult(SlugNotUnique);
            }

            return Unit.Instance;
        }
        
        private static IIncludableQueryable<Methodology, Publication> HydrateMethodologyForMethodologySummaryViewModel(IQueryable<Methodology> queryable)
        {
            return queryable
                .Include(methodology => methodology.MethodologyParent)
                .ThenInclude(mp => mp.Publications)
                .ThenInclude(pm => pm.Publication);
        }

        private static IQueryable<Methodology> HydrateMethodologyForManageMethodologySummaryViewModel(
            IQueryable<Methodology> queryable)
        {
            return queryable.Include(methodology => methodology.MethodologyParent);
        }
    }
}
