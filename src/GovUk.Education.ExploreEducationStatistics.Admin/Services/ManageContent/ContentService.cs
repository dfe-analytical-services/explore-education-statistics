#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class ContentService : IContentService
    {
        private readonly ContentDbContext _context;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public ContentService(ContentDbContext context,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            IMapper mapper)
        {
            _context = context;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _mapper = mapper;
        }

        public Task<Either<ActionResult, List<ContentSectionViewModel>>> GetContentSections(
            Guid releaseId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId, HydrateContentSectionsAndBlocks)
                .OnSuccess(OrderedContentSections);
        }

        public Task<Either<ActionResult, List<ContentSectionViewModel>>> ReorderContentSections(
            Guid releaseId,
            Dictionary<Guid, int> newSectionOrder)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId, HydrateContentSectionsAndBlocks)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    foreach (var (sectionId, newOrder) in newSectionOrder)
                    {
                        var contentSection = release
                            .GenericContent
                            .ToList()
                            .SingleOrDefault(section => section.Id == sectionId);

                        if (contentSection != null)
                        {
                            contentSection.Order = newOrder;
                        }
                    }

                    _context.Releases.Update(release);
                    await _context.SaveChangesAsync();
                    return OrderedContentSections(release);
                });
        }

        public Task<Either<ActionResult, ContentSectionViewModel>> AddContentSection(
            Guid releaseId,
            ContentSectionAddRequest? request)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId, HydrateContentSectionsAndBlocks)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    var orderForNewSection = request?.Order ??
                                             release.GenericContent.Max(contentSection => contentSection.Order) + 1;

                    release.GenericContent
                        .ToList()
                        .FindAll(contentSection => contentSection.Order >= orderForNewSection)
                        .ForEach(contentSection => contentSection.Order++);

                    var newContentSection = new ReleaseContentSection
                    {
                        Heading = "New section",
                        Order = orderForNewSection
                    };

                    release.Content.Add(newContentSection);

                    _context.Releases.Update(release);
                    await _context.SaveChangesAsync();
                    return _mapper.Map<ContentSectionViewModel>(newContentSection);
                });
        }

        public Task<Either<ActionResult, ContentSectionViewModel>> UpdateContentSectionHeading(
            Guid contentSectionId,
            string newHeading)
        {
            return _persistenceHelper
                .CheckEntityExists<ReleaseContentSection>(contentSectionId, HydrateContentSectionsAndBlocks)
                .OnSuccess(CheckCanUpdateContentSection)
                .OnSuccess(async contentSection =>
                {
                    _context.ContentSections.Update(contentSection);
                    contentSection.Heading = newHeading;
                    await _context.SaveChangesAsync();
                    return BuildContentSectionViewModel(contentSection);
                });
        }

        public Task<Either<ActionResult, List<ContentSectionViewModel>>> RemoveContentSection(
            Guid releaseId,
            Guid contentSectionId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId, HydrateContentSectionsAndBlocks)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess<ActionResult, Release, List<ContentSectionViewModel>>(async release =>
                {
                    var sectionToRemove = release
                        .Content
                        .SingleOrDefault(contentSection => contentSection.Id == contentSectionId);

                    if (sectionToRemove == null)
                    {
                        return new NotFoundResult();
                    }

                    release.Content.Remove(sectionToRemove);
                    _context.ContentSections.Remove(sectionToRemove);

                    var removedSectionOrder = sectionToRemove.Order;

                    release.GenericContent
                        .ToList()
                        .FindAll(contentSection => contentSection.Order > removedSectionOrder)
                        .ForEach(contentSection => contentSection.Order--);

                    _context.Releases.Update(release);
                    await _context.SaveChangesAsync();
                    return OrderedContentSections(release);
                });
        }

        public Task<Either<ActionResult, ContentSectionViewModel>> GetContentSection(Guid contentSectionId)
        {
            return _persistenceHelper
                .CheckEntityExists<ReleaseContentSection>(contentSectionId, HydrateContentSectionsAndBlocks)
                .OnSuccess(BuildContentSectionViewModel);
        }

        public Task<Either<ActionResult, List<IContentBlockViewModel>>> ReorderContentBlocks(
            Guid releaseId,
            Guid contentSectionId,
            Dictionary<Guid, int> newBlocksOrder)
        {
            return _persistenceHelper
                .CheckEntityExists<ReleaseContentSection>(contentSectionId, HydrateContentSectionsAndBlocks)
                .OnSuccess(CheckCanUpdateContentSection)
                .OnSuccess(async contentSection =>
                {
                    foreach (var (blockId, newOrder) in newBlocksOrder)
                    {
                        var contentBlock = contentSection.Content.SingleOrDefault(block => block.Id == blockId);
                        if (contentBlock != null)
                        {
                            contentBlock.Order = newOrder;
                        }
                    }

                    _context.ContentSections.Update(contentSection);
                    await _context.SaveChangesAsync();
                    return OrderedContentBlocks(contentSection);
                });
        }

        public Task<Either<ActionResult, IContentBlockViewModel>> AddContentBlock(
            Guid contentSectionId,
            ContentBlockAddRequest request)
        {
            return _persistenceHelper
                .CheckEntityExists<ReleaseContentSection>(contentSectionId)
                .OnSuccess(CheckCanUpdateContentSection)
                .OnSuccess(async contentSection =>
                {
                    var newContentBlock = new ReleaseContentBlock
                    {
                        Created = DateTime.UtcNow
                    };
                    return await AddContentBlockToContentSectionAndSave(request.Order, contentSection, newContentBlock);
                });
        }

        public Task<Either<ActionResult, List<IContentBlockViewModel>>> RemoveContentBlock(
            Guid releaseId,
            Guid contentSectionId,
            Guid contentBlockId)
        {
            return _persistenceHelper
                .CheckEntityExists<ReleaseContentSection>(contentSectionId, HydrateContentSectionsAndBlocks)
                .OnSuccess(CheckCanUpdateContentSection)
                .OnSuccess<ActionResult, ReleaseContentSection, List<IContentBlockViewModel>>(async contentSection =>
                {
                    var blockToRemove = contentSection.Content.SingleOrDefault(block => block.Id == contentBlockId);

                    if (blockToRemove == null)
                    {
                        return ValidationActionResult(ContentBlockNotAttachedToThisContentSection);
                    }

                    RemoveContentBlockFromContentSection(contentSection, blockToRemove);

                    _context.ContentSections.Update(contentSection);
                    await _context.SaveChangesAsync();
                    return OrderedContentBlocks(contentSection);
                });
        }

        public Task<Either<ActionResult, DataBlockViewModel>> UpdateDataBlock(
            Guid releaseId, Guid contentSectionId, Guid dataBlockId, DataBlockUpdateRequest request)
        {
            return _persistenceHelper
                .CheckEntityExists<DataBlock>(dataBlockId)
                // TODO EES-2168 check can update release
                .OnSuccess(async blockToUpdate =>
                {
                    blockToUpdate.Summary ??= new DataBlockSummary();

                    blockToUpdate.Summary.DataDefinitionTitle = new List<string>
                    {
                        request.DataDefinitionTitle
                    };

                    blockToUpdate.Summary.DataDefinition = new List<string>
                    {
                        request.DataDefinition
                    };

                    blockToUpdate.Summary.DataSummary = new List<string>
                    {
                        request.DataSummary
                    };

                    _context.DataBlocks.Update(blockToUpdate);
                    await _context.SaveChangesAsync();
                    return _mapper.Map<DataBlockViewModel>(blockToUpdate);
                });
        }

        public Task<Either<ActionResult, IContentBlockViewModel>> UpdateContentBlock(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, ContentBlockUpdateRequest request)
        {
            return _persistenceHelper
                .CheckEntityExists<ReleaseContentBlock>(contentBlockId)
                // TODO EES-2168 check can update content block
                .OnSuccess(async blockToUpdate =>
                {
                    blockToUpdate.Body = request.Body;
                    _context.ReleaseContentBlocks.Update(blockToUpdate);
                    await _context.SaveChangesAsync();
                    return _mapper.Map<IContentBlockViewModel>(blockToUpdate);
                });
        }

        public async Task<Either<ActionResult, List<DataBlock>>> GetUnattachedDataBlocks(Guid releaseId)
        {
            return await _context.DataBlocks
                .Where(dataBlock => dataBlock.ReleaseId == releaseId && dataBlock.ContentSectionId == null)
                .OrderBy(dataBlock => dataBlock.Name)
                .ToListAsync();
        }

        public Task<Either<ActionResult, IContentBlockViewModel>> AttachDataBlock(Guid contentSectionId,
            DataBlockAttachRequest request)
        {
            return _persistenceHelper
                .CheckEntityExists<ReleaseContentSection>(contentSectionId, HydrateContentSectionsAndBlocks)
                .OnSuccess(CheckCanUpdateContentSection)
                .OnSuccessCombineWith(contentSection => _persistenceHelper.CheckEntityExists<DataBlock>(
                    q => q.Where(dataBlock =>
                        contentSection.ReleaseId == dataBlock.ReleaseId &&
                        dataBlock.Id == request.DataBlockId)))
                .OnSuccess(async contentSectionAndDataBlock =>
                {
                    var (contentSection, dataBlock) = contentSectionAndDataBlock;

                    // Create a new ContentBlock for the DataBlock
                    var contentBlock = new ReleaseContentBlock
                    {
                        DataBlockId = dataBlock.Id
                    };

                    return await AddContentBlockToContentSectionAndSave(request.Order, contentSection, contentBlock);
                });
        }

        private async Task<Either<ActionResult, IContentBlockViewModel>> AddContentBlockToContentSectionAndSave(
            int? order, ReleaseContentSection section, ReleaseContentBlock newContentBlock)
        {
            var orderForNewBlock = OrderValueForNewlyAddedContentBlock(order, section);

            section.Content
                .FindAll(contentBlock => contentBlock.Order >= orderForNewBlock)
                .ForEach(contentBlock => contentBlock.Order++);

            newContentBlock.Order = orderForNewBlock;
            section.Content.Add(newContentBlock);

            _context.ContentSections.Update(section);
            await _context.SaveChangesAsync();
            return new Either<ActionResult, IContentBlockViewModel>(
                _mapper.Map<IContentBlockViewModel>(newContentBlock));
        }

        private static int OrderValueForNewlyAddedContentBlock(int? order, ReleaseContentSection contentSection)
        {
            if (order.HasValue)
            {
                return order.Value;
            }

            // TODO EES-2168 is this right?
            if (!contentSection.Content.Any())
            {
                return contentSection.Content.Max(contentBlock => contentBlock.Order) + 1;
            }

            return 1;
        }

        private void RemoveContentBlockFromContentSection(
            ReleaseContentSection section,
            ReleaseContentBlock blockToRemove)
        {
            section.Content.Remove(blockToRemove);

            var removedBlockOrder = blockToRemove.Order;

            section.Content
                .FindAll(contentBlock => contentBlock.Order > removedBlockOrder)
                .ForEach(contentBlock => contentBlock.Order--);

            _context.ContentBlocks.Remove(blockToRemove);
        }

        private List<IContentBlockViewModel> OrderedContentBlocks(ReleaseContentSection contentSection)
        {
            return _mapper.Map<List<IContentBlockViewModel>>(contentSection
                .Content
                .OrderBy(block => block.Order)
                .ToList());
        }

        private List<ContentSectionViewModel> OrderedContentSections(Release release)
        {
            return _mapper.Map<List<ContentSectionViewModel>>(release.GenericContent)
                .OrderBy(c => c.Order)
                .ToList();
        }

        private ContentSectionViewModel BuildContentSectionViewModel(ReleaseContentSection contentSection)
        {
            return _mapper.Map<ContentSectionViewModel>(contentSection);
        }

        private static IQueryable<Release> HydrateContentSectionsAndBlocks(IQueryable<Release> values)
        {
            return values
                .Include(r => r.Content)
                .ThenInclude(r => r.Content)
                .ThenInclude(content => content.Comments)
                .ThenInclude(comment => comment.CreatedBy)
                .Include(r => r.Content)
                .ThenInclude(r => r.Content)
                .ThenInclude(content => content.Comments)
                .ThenInclude(comment => comment.ResolvedBy);
        }

        private static IQueryable<ReleaseContentSection> HydrateContentSectionsAndBlocks(
            IQueryable<ReleaseContentSection> values)
        {
            return values
                .Include(r => r.Release)
                .Include(r => r.Content)
                .ThenInclude(content => content.Comments)
                .ThenInclude(comment => comment.CreatedBy)
                .Include(r => r.Content)
                .ThenInclude(content => content.Comments)
                .ThenInclude(comment => comment.ResolvedBy);
        }

        private Task<Either<ActionResult, ReleaseContentSection>> CheckCanUpdateContentSection(
            ReleaseContentSection contentSection)
        {
            return _userService
                .CheckCanUpdateRelease(contentSection.Release)
                .OnSuccess(_ => contentSection);
        }
    }
}
