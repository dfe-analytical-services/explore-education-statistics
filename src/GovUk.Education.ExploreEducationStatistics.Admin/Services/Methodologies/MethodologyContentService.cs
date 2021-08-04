using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
 using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
 using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
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

        private static readonly Dictionary<ContentListType, Func<Methodology, List<ContentSection>>> ContentListSelector
            = new Dictionary<ContentListType, Func<Methodology, List<ContentSection>>>
            {
                {ContentListType.Content, methodology => methodology.Content},
                {ContentListType.Annexes, methodology => methodology.Annexes},
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

        public Task<Either<ActionResult, ManageMethodologyContentViewModel>> GetContent(Guid methodologyId)
        {
            return _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId, HydrateMethodologyForManageMethodologyContentViewModel)
                .OnSuccess(CheckCanViewMethodology)
                .OnSuccess(_mapper.Map<ManageMethodologyContentViewModel>);
        }

        public Task<Either<ActionResult, List<T>>> GetContentBlocks<T>(Guid methodologyId) where T : ContentBlock
        {
            return _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId)
                .OnSuccess(CheckCanViewMethodology)
                .OnSuccess(methodology =>
                {
                    var sections =
                        (methodology.Annexes)
                        .Concat(methodology.Content);

                    return sections
                        .SelectMany(section => section.Content)
                        .OfType<T>()
                        .ToList();
                });
        }

        public Task<Either<ActionResult, List<ContentSectionViewModel>>> GetContentSections(
            Guid methodologyId, ContentListType contentType)
        {
            return _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId)
                .OnSuccess(CheckCanViewMethodology)
                .OnSuccess(methodology =>
                {
                    var content = ContentListSelector[contentType](methodology);
                    return OrderedContentSections(content);
                });
        }

        public Task<Either<ActionResult, ContentSectionViewModel>> GetContentSection(Guid methodologyId,
            Guid contentSectionId)
        {
            return
                CheckContentSectionExists(methodologyId, contentSectionId)
                    .OnSuccess(CheckCanViewMethodology)
                    .OnSuccess(tuple => _mapper.Map<ContentSectionViewModel>(tuple.Item2));
        }

        public Task<Either<ActionResult, List<ContentSectionViewModel>>> ReorderContentSections(
            Guid methodologyId,
            Dictionary<Guid, int> newSectionOrder)
        {
            return _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId)
                .OnSuccess(CheckCanUpdateMethodology)
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

        public Task<Either<ActionResult, ContentSectionViewModel>> AddContentSection(
            Guid methodologyId, ContentSectionAddRequest request, ContentListType contentType)
        {
            return _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId)
                .OnSuccess(CheckCanUpdateMethodology)
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
                    return _mapper.Map<ContentSectionViewModel>(newContentSection);
                });
        }

        public Task<Either<ActionResult, ContentSectionViewModel>> UpdateContentSectionHeading(
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
                        return _mapper.Map<ContentSectionViewModel>(sectionToUpdate);
                    });
        }

        public Task<Either<ActionResult, List<ContentSectionViewModel>>> RemoveContentSection(
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

        public Task<Either<ActionResult, List<IContentBlockViewModel>>> ReorderContentBlocks(Guid methodologyId,
            Guid contentSectionId, Dictionary<Guid, int> newBlocksOrder)
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

        public Task<Either<ActionResult, IContentBlockViewModel>> AddContentBlock(Guid methodologyId,
            Guid contentSectionId,
            ContentBlockAddRequest request)
        {
            return CheckContentSectionExists(methodologyId, contentSectionId)
                .OnSuccess(CheckCanUpdateMethodology)
                .OnSuccess(EnsureContentBlockListNotNull)
                .OnSuccess(async tuple =>
                {
                    var (methodology, section) = tuple;
                    var newContentBlock = CreateContentBlockForType(request.Type);
                    return await AddContentBlockToContentSectionAndSave(
                        request.Order, section, newContentBlock, methodology);
                });
        }

        public Task<Either<ActionResult, List<IContentBlockViewModel>>> RemoveContentBlock(
            Guid methodologyId, Guid contentSectionId, Guid contentBlockId)
        {
            return CheckContentSectionExists(methodologyId, contentSectionId)
                .OnSuccess(CheckCanUpdateMethodology)
                .OnSuccess(EnsureContentBlockListNotNull)
                .OnSuccess(async tuple =>
                {
                    var (methodology, section) = tuple;

                    var blockToRemove = section.Content.Find(block => block.Id == contentBlockId);

                    if (blockToRemove == null)
                    {
                        return NotFound<List<IContentBlockViewModel>>();
                    }

                    RemoveContentBlockFromContentSection(section, blockToRemove);

                    _context.Methodologies.Update(methodology);
                    await _context.SaveChangesAsync();
                    return OrderedContentBlocks(section);
                });
        }

        public Task<Either<ActionResult, IContentBlockViewModel>> UpdateTextBasedContentBlock(
            Guid methodologyId, Guid contentSectionId, Guid contentBlockId, ContentBlockUpdateRequest request)
        {
            return CheckContentSectionExists(methodologyId, contentSectionId)
                .OnSuccess(CheckCanUpdateMethodology)
                .OnSuccess(EnsureContentBlockListNotNull)
                .OnSuccess(async tuple =>
                {
                    var (methodology, section) = tuple;

                    var blockToUpdate = section.Content.Find(block => block.Id == contentBlockId);

                    if (blockToUpdate == null)
                    {
                        return NotFound<IContentBlockViewModel>();
                    }

                    return blockToUpdate switch
                    {
                        HtmlBlock htmlBlock => await UpdateHtmlBlock(htmlBlock, request.Body, methodology),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                });
        }

        private async Task<Either<ActionResult, IContentBlockViewModel>> AddContentBlockToContentSectionAndSave(int? order,
            ContentSection section,
            ContentBlock newContentBlock, Methodology methodology)
        {
            var orderForNewBlock = OrderValueForNewlyAddedContentBlock(order, section);

            section.Content
                .FindAll(contentBlock => contentBlock.Order >= orderForNewBlock)
                .ForEach(contentBlock => contentBlock.Order++);

            newContentBlock.Order = orderForNewBlock;
            section.Content.Add(newContentBlock);

            _context.Methodologies.Update(methodology);
            await _context.SaveChangesAsync();
            return new Either<ActionResult, IContentBlockViewModel>(_mapper.Map<IContentBlockViewModel>(newContentBlock));
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
            ContentBlock blockToRemove)
        {
            section.Content.Remove(blockToRemove);

            var removedBlockOrder = blockToRemove.Order;

            section.Content
                .FindAll(contentBlock => contentBlock.Order > removedBlockOrder)
                .ForEach(contentBlock => contentBlock.Order--);
        }

        private async Task<Either<ActionResult, IContentBlockViewModel>> UpdateHtmlBlock(HtmlBlock blockToUpdate,
            string body, Methodology methodology)
        {
            blockToUpdate.Body = body;
            return await SaveMethodology<HtmlBlockViewModel>(blockToUpdate, methodology);
        }

        private async Task<T> SaveMethodology<T>(ContentBlock block, Methodology methodology) 
            where T: IContentBlockViewModel
        {
            _context.Update(methodology);
            await _context.SaveChangesAsync();
            return _mapper.Map<T>(block);
        }

        private static ContentBlock CreateContentBlockForType(ContentBlockType type)
        {
            var classType = GetContentBlockClassTypeFromEnumValue(type);
            var newContentBlock = (ContentBlock) Activator.CreateInstance(classType);
            newContentBlock.Id = Guid.NewGuid();
            newContentBlock.Created = DateTime.UtcNow;
            return newContentBlock;
        }

        private List<IContentBlockViewModel> OrderedContentBlocks(ContentSection section)
        {
            return _mapper.Map<List<IContentBlockViewModel>>(section
                .Content
                .OrderBy(block => block.Order)
                .ToList());
        }

        private List<ContentSectionViewModel> OrderedContentSections(List<ContentSection> sectionsList)
        {
            return _mapper.Map<List<ContentSectionViewModel>>(sectionsList)
                .OrderBy(c => c.Order)
                .ToList();
        }

        private Task<Either<ActionResult, Tuple<Methodology, ContentSection>>> CheckContentSectionExists(
            Guid methodologyId, Guid contentSectionId)
        {
            return _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId)
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
            if (methodology.Status != MethodologyStatus.Draft)
            {
                return Task.FromResult<Either<ActionResult, Methodology>>(
                    ValidationActionResult(ValidationErrorMessages.MethodologyMustBeDraft));
            }

            return _userService.CheckCanUpdateMethodology(methodology);
        }

        private Task<Either<ActionResult, Tuple<Methodology, ContentSection>>> CheckCanUpdateMethodology(
            Tuple<Methodology, ContentSection> tuple)
        {
            if (tuple.Item1.Status != MethodologyStatus.Draft)
            {
                return Task.FromResult<Either<ActionResult, Tuple<Methodology, ContentSection>>>(
                    ValidationActionResult(ValidationErrorMessages.MethodologyMustBeDraft));
            }

            return _userService
                .CheckCanUpdateMethodology(tuple.Item1)
                .OnSuccess(_ => tuple);
        }

        private Either<ActionResult, Tuple<Methodology, ContentSection>> EnsureContentBlockListNotNull(
            Tuple<Methodology, ContentSection> tuple)
        {
            if (tuple.Item2.Content == null)
            {
                tuple.Item2.Content = new List<ContentBlock>();
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

        private Either<ActionResult, Tuple<Methodology, List<ContentSection>>> FindContentList(Methodology methodology,
            List<Guid> contentSectionIds)
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

        private static bool ContentListContainsAllSectionIds(List<ContentSection> sectionsList,
            List<Guid> contentSectionIds)
        {
            if (sectionsList == null)
            {
                return false;
            }

            var sectionsListIds = sectionsList.Select(section => section.Id);
            return contentSectionIds.All(id => sectionsListIds.Contains(id));
        }

        private static IIncludableQueryable<Methodology, MethodologyParent> 
            HydrateMethodologyForManageMethodologyContentViewModel(IQueryable<Methodology> query)
        {
            // Load the MethodologyParent so that Methodology Title and Slug can be provided by the MethodologyParent.
            return query.Include(m => m.MethodologyParent);
        }
    }
}
