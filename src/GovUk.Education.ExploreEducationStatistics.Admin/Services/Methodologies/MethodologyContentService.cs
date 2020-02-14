using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ContentBlockUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class MethodologyContentService : IMethodologyContentService
    {
        private readonly ContentDbContext _context;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper; 
        private readonly IUserService _userService; 

        public MethodologyContentService(ContentDbContext context, 
            IPersistenceHelper<ContentDbContext> persistenceHelper, 
            IUserService userService)
        {
            _context = context;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
        }

        public Task<Either<ActionResult, List<ContentSectionViewModel>>> GetContentSectionsAsync(
            Guid methodologyId)
        {
            return _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId)
                .OnSuccess(EnsureMethodologyContentNotNull)
                .OnSuccess(methodology =>
                {
                    if (methodology.Content == null)
                    {
                        return new List<ContentSectionViewModel>();
                    }

                    return methodology
                        .Content
                        .Select(ContentSectionViewModel.ToViewModel)
                        .OrderBy(c => c.Order)
                        .ToList();
                });
        }

        public Task<Either<ActionResult, List<ContentSectionViewModel>>> ReorderContentSectionsAsync(
            Guid methodologyId, 
            Dictionary<Guid, int> newSectionOrder)
        {
            return _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId)
                // .OnSuccess(CheckCanUpdateMethodology)
                .OnSuccess(EnsureMethodologyContentNotNull)
                .OnSuccess(async methodology =>
                {
                    newSectionOrder.ToList().ForEach(kvp =>
                    {
                        var (sectionId, newOrder) = kvp;
                        methodology
                            .Content
                            .Find(section => section.Id == sectionId).Order = newOrder;
                    });
                    
                    _context.Methodologies.Update(methodology);
                    await _context.SaveChangesAsync();
                    return OrderedContentSections(methodology);
                });
        }

        public Task<Either<ActionResult, ContentSectionViewModel>> AddContentSectionAsync(
            Guid methodologyId, AddContentSectionRequest? request)
        {
            return _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId)
                // .OnSuccess(CheckCanUpdateMethodology)
                .OnSuccess(EnsureMethodologyContentNotNull)
                .OnSuccess(async methodology =>
                {
                    var orderForNewSection = request?.Order ?? 
                                             methodology.Content.Max(contentSection => contentSection.Order) + 1;
                    
                    methodology.Content
                        .FindAll(contentSection => contentSection.Order >= orderForNewSection)
                        .ForEach(contentSection => contentSection.Order++);

                    var newContentSection = new ContentSection
                    {
                        Id = Guid.NewGuid(),
                        Heading = "New section",
                        Order = orderForNewSection
                    };
                    
                    methodology.Content.Add(newContentSection);
                    
                    _context.Methodologies.Update(methodology);
                    await _context.SaveChangesAsync();
                    return ContentSectionViewModel.ToViewModel(newContentSection);
                });
        }

        public Task<Either<ActionResult, ContentSectionViewModel>> UpdateContentSectionHeadingAsync(
            Guid methodologyId, Guid contentSectionId, string newHeading)
        {
            return 
                CheckContentSectionExistsActionResult(methodologyId, contentSectionId)
                    // .OnSuccess(CheckCanUpdateMethodology)
                    .OnSuccess(EnsureMethodologyContentNotNull)
                    .OnSuccess(async tuple =>
                    {
                        var (methodology, sectionToUpdate) = tuple;
                        
                        sectionToUpdate.Heading = newHeading;

                        _context.Methodologies.Update(methodology);
                        await _context.SaveChangesAsync();
                        return ContentSectionViewModel.ToViewModel(sectionToUpdate);
                    });
        }
        
        public  Task<Either<ActionResult, List<ContentSectionViewModel>>> RemoveContentSectionAsync(
            Guid methodologyId,
            Guid contentSectionId)
        {
            return 
                CheckContentSectionExistsActionResult(methodologyId, contentSectionId)
                    // .OnSuccess(CheckCanUpdateMethodology)
                    .OnSuccess(EnsureMethodologyContentNotNull)
                    .OnSuccess(async tuple =>
                    {
                        var (methodology, sectionToRemove) = tuple;
                        
                        methodology.Content.Remove(sectionToRemove);

                        var removedSectionOrder = sectionToRemove.Order;
                        
                        methodology.Content
                            .FindAll(contentSection => contentSection.Order > removedSectionOrder)
                            .ForEach(contentSection => contentSection.Order--);
                        
                        _context.Methodologies.Update(methodology);
                        await _context.SaveChangesAsync();
                        return OrderedContentSections(methodology);
                    });
        }

        public Task<Either<ActionResult, ContentSectionViewModel>> GetContentSectionAsync(Guid methodologyId, Guid contentSectionId)
        {
            return 
                CheckContentSectionExistsActionResult(methodologyId, contentSectionId)
                    .OnSuccess(tuple => ContentSectionViewModel.ToViewModel(tuple.Item2));
        }

        public Task<Either<ActionResult, List<IContentBlock>>> ReorderContentBlocksAsync(Guid methodologyId, Guid contentSectionId, Dictionary<Guid, int> newBlocksOrder)
        {
            return 
                CheckContentSectionExistsActionResult(methodologyId, contentSectionId)
                    // .OnSuccess(CheckCanUpdateMethodology)
                    .OnSuccess(EnsureMethodologyContentNotNull)
                    .OnSuccess(async tuple =>
                    {
                        var (methodology, section) = tuple;

                        newBlocksOrder.ToList().ForEach(kvp =>
                        {
                            var (blockId, newOrder) = kvp;
                            section.Content.Find(block => block.Id == blockId).Order = newOrder;
                        });
                        
                        _context.Methodologies.Update(methodology);
                        await _context.SaveChangesAsync();
                        return OrderedContentBlocks(section);
                    });
        }

        public Task<Either<ActionResult, IContentBlock>> AddContentBlockAsync(Guid methodologyId, Guid contentSectionId,
            AddContentBlockRequest request) 
        {
            return 
                CheckContentSectionExistsActionResult(methodologyId, contentSectionId)
                    // .OnSuccess(CheckCanUpdateMethodology)
                    .OnSuccess(EnsureMethodologyContentNotNull)
                    .OnSuccess(async tuple =>
                    {
                        var (methodology, section) = tuple;
                        var newContentBlock = CreateContentBlockForType(request.Type);
                        return await AddContentBlockToContentSectionAndSaveAsync(request.Order, section, newContentBlock, methodology);
                    });
        }

        public Task<Either<ActionResult, List<IContentBlock>>> RemoveContentBlockAsync(
            Guid methodologyId, Guid contentSectionId, Guid contentBlockId)
        {
            return 
                CheckContentSectionExistsActionResult(methodologyId, contentSectionId)
                    // .OnSuccess(CheckCanUpdateMethodology)
                    .OnSuccess(EnsureMethodologyContentNotNull)
                    .OnSuccess(async tuple =>
                    {
                        var (methodology, section) = tuple;

                        var blockToRemove = section.Content.Find(block => block.Id == contentBlockId);

                        if (blockToRemove == null)
                        {
                            return NotFound<List<IContentBlock>>(); 
                        }
                        
                        RemoveContentBlockFromContentSection(section, blockToRemove);
                        
                        _context.Methodologies.Update(methodology);
                        await _context.SaveChangesAsync();
                        return OrderedContentBlocks(section);
                    });
        }

        public Task<Either<ActionResult, IContentBlock>> UpdateTextBasedContentBlockAsync(
            Guid methodologyId, Guid contentSectionId, Guid contentBlockId, UpdateTextBasedContentBlockRequest request)
        {
            return 
                CheckContentSectionExistsActionResult(methodologyId, contentSectionId)
                    // .OnSuccess(CheckCanUpdateMethodology)
                    .OnSuccess(EnsureMethodologyContentNotNull)
                    .OnSuccess(async tuple =>
                    {
                        var (methodology, section) = tuple;

                        var blockToUpdate = section.Content.Find(block => block.Id == contentBlockId);

                        if (blockToUpdate == null)
                        {
                            return NotFound<IContentBlock>();
                        }

                        switch (Enum.Parse<ContentBlockType>(blockToUpdate.Type))
                        {
                            case ContentBlockType.MarkDownBlock:
                                return await UpdateMarkDownBlock((MarkDownBlock) blockToUpdate, request.Body, methodology);
                            case ContentBlockType.HtmlBlock:
                                return await UpdateHtmlBlock((HtmlBlock) blockToUpdate, request.Body, methodology);
                            case ContentBlockType.InsetTextBlock:
                                return await UpdateInsetTextBlock((InsetTextBlock) blockToUpdate, request.Heading, request.Body, methodology);
                            case ContentBlockType.DataBlock:
                                return ValidationActionResult(IncorrectContentBlockTypeForUpdate);
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    });
        }
        
        private async Task<Either<ActionResult, IContentBlock>> AddContentBlockToContentSectionAndSaveAsync(int? order, ContentSection section,
            IContentBlock newContentBlock, Methodology methodology)
        {
            if (section.Content == null)
            {
                section.Content = new List<IContentBlock>();
            }

            var orderForNewBlock = OrderValueForNewlyAddedContentBlock(order, section);

            section.Content
                .FindAll(contentBlock => contentBlock.Order >= orderForNewBlock)
                .ForEach(contentBlock => contentBlock.Order++);

            newContentBlock.Order = orderForNewBlock;
            section.Content.Add(newContentBlock);

            _context.Methodologies.Update(methodology);
            await _context.SaveChangesAsync();
            return newContentBlock;
        }

        private static int OrderValueForNewlyAddedContentBlock(int? order, ContentSection section)
        {
            if (order.HasValue)
            {
                return (int) order;
            }

            if (!section.Content.IsNullOrEmpty())
            {
                return section.Content.Max(contentBlock => contentBlock.Order) + 1;
            }

            return 1;
        }

        private void RemoveContentBlockFromContentSection(
            ContentSection section,
            IContentBlock blockToRemove)
        {
            section.Content.Remove(blockToRemove);

            var removedBlockOrder = blockToRemove.Order;

            section.Content
                .FindAll(contentBlock => contentBlock.Order > removedBlockOrder)
                .ForEach(contentBlock => contentBlock.Order--);
        }

        private async Task<Either<ActionResult, IContentBlock>> UpdateMarkDownBlock(MarkDownBlock blockToUpdate,
            string body, Methodology methodology)
        {
            blockToUpdate.Body = body;
            return await SaveMethodology(blockToUpdate, methodology);
        }

        private async Task<Either<ActionResult, IContentBlock>> UpdateHtmlBlock(HtmlBlock blockToUpdate,
            string body, Methodology methodology)
        {
            blockToUpdate.Body = body;
            return await SaveMethodology(blockToUpdate, methodology);
        }

        private async Task<Either<ActionResult, IContentBlock>> UpdateInsetTextBlock(InsetTextBlock blockToUpdate,
            string heading, string body, Methodology methodology)
        {
            blockToUpdate.Heading = heading;
            blockToUpdate.Body = body;
            return await SaveMethodology(blockToUpdate, methodology);
        }

        private async Task<IContentBlock> SaveMethodology(IContentBlock block, Methodology methodology)
        {
            _context.Update(methodology);
            await _context.SaveChangesAsync();
            return block;
        }

        private static IContentBlock CreateContentBlockForType(ContentBlockType type)
        {
            var classType = GetContentBlockClassTypeFromEnumValue(type);
            var newContentBlock = (IContentBlock) Activator.CreateInstance(classType);
            newContentBlock.Id = Guid.NewGuid();
            return newContentBlock;
        }

        private static List<IContentBlock> OrderedContentBlocks(ContentSection section)
        {
            return section
                .Content
                .OrderBy(block => block.Order)
                .ToList();
        }

        private static List<ContentSectionViewModel> OrderedContentSections(Methodology methodology)
        {
            return methodology
                .Content
                .Select(ContentSectionViewModel.ToViewModel)
                .OrderBy(c => c.Order)
                .ToList();
        }

        private Task<Either<ActionResult, Tuple<Methodology, ContentSection>>> CheckContentSectionExistsActionResult(
            Guid methodologyId, Guid contentSectionId)
        {
            return _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId)
                .OnSuccess(EnsureMethodologyContentNotNull)
                .OnSuccess(methodology =>
                {
                    var section = methodology
                        .Content
                        .Find(contentSection => contentSection.Id == contentSectionId);

                    if (section == null)
                    {
                        return new NotFoundResult();
                    }

                    return new Either<ActionResult, Tuple<Methodology, ContentSection>>(
                        new Tuple<Methodology, ContentSection>(methodology, section));
                });
        }

        private Task<Either<ActionResult, Methodology>> CheckCanUpdateMethodology(Methodology methodology)
        {
            return _userService.CheckCanUpdateMethodology(methodology);
        }

        private Task<Either<ActionResult, Tuple<Methodology, ContentSection>>> CheckCanUpdateMethodology(
            Tuple<Methodology, ContentSection> tuple)
        {
            return _userService
                .CheckCanUpdateMethodology(tuple.Item1)
                .OnSuccess(_ => tuple);
        }

        private Either<ActionResult, Methodology> EnsureMethodologyContentNotNull(Methodology methodology)
        {
            if (methodology.Content == null)
            {
                methodology.Content = new List<ContentSection>();
            }

            return methodology;
        }

        private Either<ActionResult, Tuple<Methodology, ContentSection>> EnsureMethodologyContentNotNull(
            Tuple<Methodology, ContentSection> tuple)
        {
            if (tuple.Item1.Content == null)
            {
                tuple.Item1.Content = new List<ContentSection>();
            }

            return tuple;
        }
    }
}