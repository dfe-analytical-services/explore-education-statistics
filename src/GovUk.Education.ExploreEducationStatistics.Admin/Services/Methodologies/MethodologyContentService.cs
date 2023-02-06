#nullable enable
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
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
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

        private static readonly Dictionary<ContentListType, Func<MethodologyVersionContent, List<ContentSection>>> ContentListSelector
            = new()
            {
                {ContentListType.Content, methodologyContent => methodologyContent.Content},
                {ContentListType.Annexes, methodologyContent => methodologyContent.Annexes},
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

        public Task<Either<ActionResult, ManageMethodologyContentViewModel>> GetContent(Guid methodologyVersionId)
        {
            return _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(methodologyVersionId, q => q
                    .Include(version => version.Notes)
                    .Include(version => version.MethodologyContent)
                    // Load the Methodology so that Slug can be provided
                    .Include(m => m.Methodology))
                .OnSuccess(CheckCanViewMethodology)
                .OnSuccess(_mapper.Map<ManageMethodologyContentViewModel>);
        }

        public Task<Either<ActionResult, List<T>>> GetContentBlocks<T>(Guid methodologyVersionId) where T : ContentBlock
        {
            return _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(methodologyVersionId, q =>
                    q.Include(version => version.MethodologyContent))
                .OnSuccess(CheckCanViewMethodology)
                .OnSuccess(methodology =>
                {
                    var sections =
                        methodology.MethodologyContent.Annexes
                            .Concat(methodology.MethodologyContent.Content);

                    return sections
                        .SelectMany(section => section.Content)
                        .OfType<T>()
                        .ToList();
                });
        }

        public Task<Either<ActionResult, ContentSectionViewModel>> GetContentSection(
            Guid methodologyVersionId,
            Guid contentSectionId)
        {
            return
                CheckContentSectionExists(methodologyVersionId, contentSectionId)
                    .OnSuccess(CheckCanViewMethodology)
                    .OnSuccess(tuple => _mapper.Map<ContentSectionViewModel>(tuple.Item2));
        }

        public Task<Either<ActionResult, List<ContentSectionViewModel>>> ReorderContentSections(
            Guid methodologyVersionId,
            Dictionary<Guid, int> newSectionOrder)
        {
            return _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(methodologyVersionId, q =>
                    q.Include(version => version.MethodologyContent))
                .OnSuccess(CheckCanUpdateMethodologyContent)
                .OnSuccess(methodology => FindContentListWithAllSectionIds(methodology, newSectionOrder.Keys.ToList()))
                .OnSuccess(async tuple =>
                {
                    var (methodologyContent, contentSections) = tuple;

                    newSectionOrder.ToList().ForEach(kvp =>
                    {
                        var (sectionId, newOrder) = kvp;
                        contentSections.Single(section => section.Id == sectionId)
                            .Order = newOrder;
                    });

                    _context.MethodologyContent.Update(methodologyContent);
                    await _context.SaveChangesAsync();
                    return OrderedContentSections(contentSections);
                });
        }

        public Task<Either<ActionResult, ContentSectionViewModel>> AddContentSection(
            Guid methodologyVersionId,
            ContentSectionAddRequest request,
            ContentListType contentType)
        {
            return _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(methodologyVersionId, q =>
                    q.Include(version => version.MethodologyContent))
                .OnSuccess(CheckCanUpdateMethodologyContent)
                .OnSuccess(async methodologyContent =>
                {
                    var content = ContentListSelector[contentType](methodologyContent);

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

                    _context.MethodologyContent.Update(methodologyContent);
                    await _context.SaveChangesAsync();
                    return _mapper.Map<ContentSectionViewModel>(newContentSection);
                });
        }

        public Task<Either<ActionResult, ContentSectionViewModel>> UpdateContentSectionHeading(
            Guid methodologyVersionId,
            Guid contentSectionId,
            string newHeading)
        {
            return
                CheckContentSectionExists(methodologyVersionId, contentSectionId)
                    .OnSuccess(CheckCanUpdateMethodologyContent)
                    .OnSuccess(async tuple =>
                    {
                        var (methodologyContent, sectionToUpdate) = tuple;

                        sectionToUpdate.Heading = newHeading;

                        _context.MethodologyContent.Update(methodologyContent);
                        await _context.SaveChangesAsync();
                        return _mapper.Map<ContentSectionViewModel>(sectionToUpdate);
                    });
        }

        public Task<Either<ActionResult, List<ContentSectionViewModel>>> RemoveContentSection(
            Guid methodologyVersionId,
            Guid contentSectionId)
        {
            return CheckContentSectionExists(methodologyVersionId, contentSectionId)
                .OnSuccess(CheckCanUpdateMethodologyContent)
                .OnSuccess(tuple => FindContentListWithSectionId(tuple.Item1, contentSectionId))
                .OnSuccess(async tuple =>
                {
                    var (methodologyContent, contentSections) = tuple;
                    var sectionToRemove = contentSections.Single(section => section.Id == contentSectionId); 

                    contentSections.Remove(sectionToRemove);

                    var removedSectionOrder = sectionToRemove.Order;

                    contentSections
                        .FindAll(contentSection => contentSection.Order > removedSectionOrder)
                        .ForEach(contentSection => contentSection.Order--);

                    _context.MethodologyContent.Update(methodologyContent);
                    await _context.SaveChangesAsync();
                    return OrderedContentSections(contentSections);
                });
        }

        public Task<Either<ActionResult, List<IContentBlockViewModel>>> ReorderContentBlocks(
            Guid methodologyVersionId,
            Guid contentSectionId,
            Dictionary<Guid, int> newBlocksOrder)
        {
            return
                CheckContentSectionExists(methodologyVersionId, contentSectionId)
                    .OnSuccess(CheckCanUpdateMethodologyContent)
                    .OnSuccess(EnsureContentBlockListNotNull)
                    .OnSuccess(async tuple =>
                    {
                        var (methodologyContent, section) = tuple;

                        newBlocksOrder.ToList().ForEach(kvp =>
                        {
                            var (blockId, newOrder) = kvp;
                            section.Content.Single(block => block.Id == blockId).Order = newOrder;
                        });

                        _context.MethodologyContent.Update(methodologyContent);
                        await _context.SaveChangesAsync();
                        return OrderedContentBlocks(section);
                    });
        }

        public Task<Either<ActionResult, IContentBlockViewModel>> AddContentBlock(
            Guid methodologyVersionId,
            Guid contentSectionId,
            ContentBlockAddRequest request)
        {
            return CheckContentSectionExists(methodologyVersionId, contentSectionId)
                .OnSuccess(CheckCanUpdateMethodologyContent)
                .OnSuccess(EnsureContentBlockListNotNull)
                .OnSuccess(async tuple =>
                {
                    var (methodologyContent, section) = tuple;
                    var newContentBlock = CreateContentBlockForType(request.Type);
                    return await AddContentBlockToContentSectionAndSave(
                        request.Order, section, newContentBlock, methodologyContent);
                });
        }

        public Task<Either<ActionResult, List<IContentBlockViewModel>>> RemoveContentBlock(
            Guid methodologyVersionId,
            Guid contentSectionId,
            Guid contentBlockId)
        {
            return CheckContentSectionExists(methodologyVersionId, contentSectionId)
                .OnSuccess(CheckCanUpdateMethodologyContent)
                .OnSuccess(EnsureContentBlockListNotNull)
                .OnSuccess(async tuple =>
                {
                    var (methodologyContent, section) = tuple;

                    var blockToRemove = section.Content.Find(block => block.Id == contentBlockId);

                    if (blockToRemove == null)
                    {
                        return NotFound<List<IContentBlockViewModel>>();
                    }

                    RemoveContentBlockFromContentSection(section, blockToRemove);

                    _context.MethodologyContent.Update(methodologyContent);
                    await _context.SaveChangesAsync();
                    return OrderedContentBlocks(section);
                });
        }

        public Task<Either<ActionResult, IContentBlockViewModel>> UpdateTextBasedContentBlock(
            Guid methodologyVersionId,
            Guid contentSectionId,
            Guid contentBlockId,
            ContentBlockUpdateRequest request)
        {
            return CheckContentSectionExists(methodologyVersionId, contentSectionId)
                .OnSuccess(CheckCanUpdateMethodologyContent)
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

        private async Task<Either<ActionResult, IContentBlockViewModel>> AddContentBlockToContentSectionAndSave(
            int? order,
            ContentSection section,
            ContentBlock newContentBlock,
            MethodologyVersionContent methodologyContent)
        {
            var orderForNewBlock = OrderValueForNewlyAddedContentBlock(order, section);

            section.Content
                .FindAll(contentBlock => contentBlock.Order >= orderForNewBlock)
                .ForEach(contentBlock => contentBlock.Order++);

            newContentBlock.Order = orderForNewBlock;
            section.Content.Add(newContentBlock);

            _context.MethodologyContent.Update(methodologyContent);
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
            string body, MethodologyVersionContent content)
        {
            blockToUpdate.Body = body;
            _context.MethodologyContent.Update(content);
            await _context.SaveChangesAsync();
            return _mapper.Map<HtmlBlockViewModel>(blockToUpdate);
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

        private Task<Either<ActionResult, Tuple<MethodologyVersion, ContentSection>>> CheckContentSectionExists(
            Guid methodologyId, Guid contentSectionId)
        {
            return _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(methodologyId, q =>
                    q.Include(version => version.MethodologyContent))
                .OnSuccess(methodology =>
                {
                    return FindContentListWithSectionId(methodology.MethodologyContent, contentSectionId)
                        .OnSuccess(tuple =>
                        {
                            var (_, contentSections) = tuple;

                            var section = contentSections
                                .Find(contentSection => contentSection.Id == contentSectionId);

                            if (section == null)
                            {
                                return new NotFoundResult();
                            }

                            return new Either<ActionResult, Tuple<MethodologyVersion, ContentSection>>(
                                TupleOf(methodology, section));
                        });
                });
        }

        private Task<Either<ActionResult, MethodologyVersion>> CheckCanViewMethodology(
            MethodologyVersion methodologyVersion)
        {
            return _userService.CheckCanViewMethodology(methodologyVersion);
        }

        private Task<Either<ActionResult, Tuple<MethodologyVersion, ContentSection>>> CheckCanViewMethodology(
            Tuple<MethodologyVersion, ContentSection> tuple)
        {
            return _userService
                .CheckCanViewMethodology(tuple.Item1)
                .OnSuccess(_ => tuple);
        }

        private async Task<Either<ActionResult, MethodologyVersionContent>> CheckCanUpdateMethodologyContent(
            MethodologyVersion methodologyVersion)
        {
            if (methodologyVersion.Status != MethodologyStatus.Draft)
            {
                return ValidationActionResult(ValidationErrorMessages.MethodologyMustBeDraft);
            }

            if (methodologyVersion.MethodologyContent == null)
            {
                throw new ArgumentException("MethodologyContent must be hydrated");
            }

            return await _userService
                .CheckCanUpdateMethodologyVersion(methodologyVersion)
                .OnSuccess(_ => methodologyVersion.MethodologyContent);
        }

        private async Task<Either<ActionResult, Tuple<MethodologyVersionContent, ContentSection>>> CheckCanUpdateMethodologyContent(
            Tuple<MethodologyVersion, ContentSection> tuple)
        {
            if (tuple.Item1.Status != MethodologyStatus.Draft)
            {
                return ValidationActionResult(ValidationErrorMessages.MethodologyMustBeDraft);
            }

            return await _userService
                .CheckCanUpdateMethodologyVersion(tuple.Item1)
                .OnSuccess(_ => new Tuple<MethodologyVersionContent, ContentSection>(tuple.Item1.MethodologyContent, tuple.Item2));
        }

        private Either<ActionResult, Tuple<MethodologyVersionContent, ContentSection>> EnsureContentBlockListNotNull(
            Tuple<MethodologyVersionContent, ContentSection> tuple)
        {
            if (tuple.Item2.Content == null)
            {
                tuple.Item2.Content = new List<ContentBlock>();
            }

            return tuple;
        }

        private static Either<ActionResult, Tuple<MethodologyVersionContent, List<ContentSection>>>
            FindContentListWithSectionId(MethodologyVersionContent content,
                Guid contentSectionId)
        {
            if (content.Content.Any(section => section.Id == contentSectionId))
            {
                return TupleOf(content, content.Content);
            }

            if (content.Annexes.Any(section => section.Id == contentSectionId))
            {
                return TupleOf(content, content.Annexes);
            }

            return new NotFoundResult();
        }

        private static Either<ActionResult, Tuple<MethodologyVersionContent, List<ContentSection>>>
            FindContentListWithAllSectionIds(MethodologyVersionContent content,
                List<Guid> contentSectionIds)
        {
            if (ComparerUtils.SequencesAreEqualIgnoringOrder(
                    content.Content.Select(section => section.Id),
                    contentSectionIds))
                {
                    return TupleOf(content, content.Content);
                }

            if (ComparerUtils.SequencesAreEqualIgnoringOrder(
                content.Annexes.Select(section => section.Id),
                contentSectionIds))
            {
                return TupleOf(content, content.Annexes);
            }

            return new NotFoundResult();
        }
    }
}
