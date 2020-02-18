using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ContentBlockUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class MethodologyContentService : IMethodologyContentService
    {
        public enum ContentListType
        {
            Content,
            Annexes
        }
        
        private static readonly Dictionary<ContentListType, Func<Methodology, List<ContentSection>>> ContentListSelector = new Dictionary<ContentListType, Func<Methodology, List<ContentSection>>>
        {
            { ContentListType.Content, methodology => methodology.Content },
            { ContentListType.Annexes, methodology => methodology.Annexes },
        };
        
        private readonly ContentDbContext _context;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper; 
        private readonly IUserService _userService; 
        private readonly IMapper _mapper; 

        public MethodologyContentService(ContentDbContext context, 
            IPersistenceHelper<ContentDbContext> persistenceHelper, 
            IUserService userService, IMapper mapper)
        {
            _context = context;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _mapper = mapper;
        }

        public Task<Either<ActionResult, ManageMethodologyContentViewModel>> GetContentAsync(Guid methodologyId)
        {
            return _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId)
                .OnSuccess(CheckCanViewMethodology)
                .OnSuccess(_mapper.Map<ManageMethodologyContentViewModel>);
        }

        public Task<Either<ActionResult, List<ContentSectionViewModel>>> GetContentSectionsAsync(
            Guid methodologyId, ContentListType contentType)
        {
            return _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId)
                .OnSuccess(CheckCanViewMethodology)
                .OnSuccess(EnsureMethodologyContentAndAnnexListsNotNull)
                .OnSuccess(methodology =>
                {
                    var content = ContentListSelector[contentType](methodology);
                    
                    return content
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
                .OnSuccess(CheckCanUpdateMethodology)
                .OnSuccess(EnsureMethodologyContentAndAnnexListsNotNull)
                .OnSuccess(methodology => FindContentList(methodology, newSectionOrder.Keys.ToList()))
                .OnSuccess(async tuple =>
                {
                    var (methodology, content) = tuple;
                    
                    newSectionOrder.ToList().ForEach(kvp =>
                    {
                        var (sectionId, newOrder) = kvp;
                        content
                            .Find(section => section.Id == sectionId)
                            .Order = newOrder;
                    });
                    
                    _context.Methodologies.Update(methodology);
                    await _context.SaveChangesAsync();
                    return OrderedContentSections(content);
                });
        }

        public Task<Either<ActionResult, ContentSectionViewModel>> AddContentSectionAsync(
            Guid methodologyId, AddContentSectionRequest? request, ContentListType contentType)
        {
            return _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId)
                .OnSuccess(CheckCanUpdateMethodology)
                .OnSuccess(EnsureMethodologyContentAndAnnexListsNotNull)
                .OnSuccess(async methodology =>
                {
                    var content = ContentListSelector[contentType](methodology);
                    
                    var orderForNewSection = request?.Order ?? 
                                             content.Max(contentSection => contentSection.Order) + 1;
                    
                    content
                        .FindAll(contentSection => contentSection.Order >= orderForNewSection)
                        .ForEach(contentSection => contentSection.Order++);

                    var newContentSection = new ContentSection
                    {
                        Id = Guid.NewGuid(),
                        Heading = "New section",
                        Order = orderForNewSection
                    };
                    
                    content.Add(newContentSection);
                    
                    _context.Methodologies.Update(methodology);
                    await _context.SaveChangesAsync();
                    return ContentSectionViewModel.ToViewModel(newContentSection);
                });
        }

        public Task<Either<ActionResult, ContentSectionViewModel>> UpdateContentSectionHeadingAsync(
            Guid methodologyId, 
            Guid contentSectionId, 
            string newHeading)
        {
            return 
                CheckContentSectionExists(methodologyId, contentSectionId)
                    .OnSuccess(CheckCanUpdateMethodology)
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
                CheckContentSectionExists(methodologyId, contentSectionId)
                    .OnSuccess(CheckCanUpdateMethodology)
                    .OnSuccess(async tuple =>
                    {
                        var (methodology, sectionToRemove) = tuple;

                        var content = FindContentList(methodology, sectionToRemove);
                        
                        content.Remove(sectionToRemove);

                        var removedSectionOrder = sectionToRemove.Order;
                        
                        content
                            .FindAll(contentSection => contentSection.Order > removedSectionOrder)
                            .ForEach(contentSection => contentSection.Order--);
                        
                        _context.Methodologies.Update(methodology);
                        await _context.SaveChangesAsync();
                        return OrderedContentSections(content);
                    });
        }

        public Task<Either<ActionResult, ContentSectionViewModel>> GetContentSectionAsync(Guid methodologyId, Guid contentSectionId)
        {
            return 
                CheckContentSectionExists(methodologyId, contentSectionId)
                    .OnSuccess(CheckCanViewMethodology)
                    .OnSuccess(tuple => ContentSectionViewModel.ToViewModel(tuple.Item2));
        }

        public Task<Either<ActionResult, List<IContentBlock>>> ReorderContentBlocksAsync(Guid methodologyId, Guid contentSectionId, Dictionary<Guid, int> newBlocksOrder)
        {
            return 
                CheckContentSectionExists(methodologyId, contentSectionId)
                    .OnSuccess(CheckCanUpdateMethodology)
                    .OnSuccess(EnsureContentBlockListNotNull)
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
                CheckContentSectionExists(methodologyId, contentSectionId)
                    .OnSuccess(CheckCanUpdateMethodology)
                    .OnSuccess(EnsureContentBlockListNotNull)
                    .OnSuccess(async tuple =>
                    {
                        var (methodology, section) = tuple;
                        var newContentBlock = CreateContentBlockForType(request.Type);
                        return await AddContentBlockToContentSectionAndSaveAsync(
                            request.Order, section, newContentBlock, methodology);
                    });
        }

        public Task<Either<ActionResult, List<IContentBlock>>> RemoveContentBlockAsync(
            Guid methodologyId, Guid contentSectionId, Guid contentBlockId)
        {
            return 
                CheckContentSectionExists(methodologyId, contentSectionId)
                    .OnSuccess(CheckCanUpdateMethodology)
                    .OnSuccess(EnsureContentBlockListNotNull)
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
                CheckContentSectionExists(methodologyId, contentSectionId)
                    .OnSuccess(CheckCanUpdateMethodology)
                    .OnSuccess(EnsureContentBlockListNotNull)
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
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    });
        }
        
        private async Task<Either<ActionResult, IContentBlock>> AddContentBlockToContentSectionAndSaveAsync(int? order,
            ContentSection section,
            IContentBlock newContentBlock, Methodology methodology)
        {
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

        private static List<ContentSectionViewModel> OrderedContentSections(List<ContentSection> sectionsList)
        {
            return sectionsList
                .Select(ContentSectionViewModel.ToViewModel)
                .OrderBy(c => c.Order)
                .ToList();
        }

        private Task<Either<ActionResult, Tuple<Methodology, ContentSection>>> CheckContentSectionExists(
            Guid methodologyId, Guid contentSectionId)
        {
            return _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId)
                .OnSuccess(EnsureMethodologyContentAndAnnexListsNotNull)
                .OnSuccess(methodology => FindContentList(methodology, contentSectionId))
                .OnSuccess(tuple =>
                {
                    var (methodology, content) = tuple;
                    
                    var section = content
                        .Find(contentSection => contentSection.Id == contentSectionId);

                    if (section == null)
                    {
                        return new NotFoundResult();
                    }

                    return new Either<ActionResult, Tuple<Methodology, ContentSection>>(
                        new Tuple<Methodology, ContentSection>(methodology, section));
                });
        }

        private Task<Either<ActionResult, Methodology>> CheckCanViewMethodology(Methodology methodology)
        {
            return _userService.CheckCanViewMethodology(methodology);
        }

        private Task<Either<ActionResult, Tuple<Methodology, ContentSection>>> CheckCanViewMethodology(
            Tuple<Methodology, ContentSection> tuple)
        {
            return _userService
                .CheckCanViewMethodology(tuple.Item1)
                .OnSuccess(_ => tuple);
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

        private Either<ActionResult, Methodology> EnsureMethodologyContentAndAnnexListsNotNull(Methodology methodology)
        {
            if (methodology.Content == null)
            {
                methodology.Content = new List<ContentSection>();
            }
            
            if (methodology.Annexes == null)
            {
                methodology.Annexes = new List<ContentSection>();
            }

            return methodology;
        }

        private Either<ActionResult, Tuple<Methodology, ContentSection>> EnsureContentBlockListNotNull(
            Tuple<Methodology, ContentSection> tuple)
        {
            if (tuple.Item2.Content == null)
            {
                tuple.Item2.Content = new List<IContentBlock>();
            }

            return tuple;
        }

        private Either<ActionResult, Tuple<Methodology, List<ContentSection>>> FindContentList(Methodology methodology,
            params Guid[] contentSectionIds)
        {
            return FindContentList(methodology, contentSectionIds.ToList());
        }
        
        private List<ContentSection> FindContentList(Methodology methodology,
            params ContentSection[] contentSections)
        {
            return FindContentList(methodology, contentSections.Select(section => section.Id).ToList()).Right.Item2;
        }

        private Either<ActionResult, Tuple<Methodology, List<ContentSection>>> FindContentList(Methodology methodology, List<Guid> contentSectionIds)
        {
            if (ContentListContainsAllSectionIds(methodology.Content, contentSectionIds))
            {
                return new Tuple<Methodology, List<ContentSection>>(methodology, methodology.Content);
            }
            
            if (ContentListContainsAllSectionIds(methodology.Annexes, contentSectionIds))
            {
                return new Tuple<Methodology, List<ContentSection>>(methodology, methodology.Annexes);
            }

            return new NotFoundResult();
        }
        
        private bool ContentListContainsAllSectionIds(List<ContentSection> sectionsList, List<Guid> contentSectionIds)
        {
            if (sectionsList == null)
            {
                return false;
            }
            
            var sectionsListIds = sectionsList.Select(section => section.Id);
            return contentSectionIds.All(id => sectionsListIds.Contains(id));
        }
    }
}