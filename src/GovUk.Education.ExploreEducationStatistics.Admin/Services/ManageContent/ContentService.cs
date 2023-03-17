using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs.Clients;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ContentBlockUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class ContentService : IContentService
    {
        private readonly ContentDbContext _context;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IKeyStatisticService _keyStatisticService;
        private readonly IReleaseContentSectionRepository _releaseContentSectionRepository;
        private readonly IContentBlockService _contentBlockService;
        private readonly IHubContext<ReleaseContentHub, IReleaseContentHubClient> _hubContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public ContentService(ContentDbContext context,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IKeyStatisticService keyStatisticService,
            IReleaseContentSectionRepository releaseContentSectionRepository,
            IContentBlockService contentBlockService,
            IHubContext<ReleaseContentHub, IReleaseContentHubClient> hubContext,
            IUserService userService,
            IMapper mapper)
        {
            _context = context;
            _persistenceHelper = persistenceHelper;
            _keyStatisticService = keyStatisticService;
            _releaseContentSectionRepository = releaseContentSectionRepository;
            _contentBlockService = contentBlockService;
            _hubContext = hubContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, List<T>>> GetContentBlocks<T>(Guid releaseId) where T : ContentBlock
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(release => _releaseContentSectionRepository.GetAllContentBlocks<T>(release.Id));
        }

        public Task<Either<ActionResult, List<ContentSectionViewModel>>> ReorderContentSections(
            Guid releaseId,
            Dictionary<Guid, int> newSectionOrder)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId, HydrateContentSectionsAndBlocks)
                .OnSuccess(CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    var contentSections = release
                        .GenericContent
                        .ToList();

                    newSectionOrder.ToList().ForEach(kvp =>
                    {
                        var (sectionId, newOrder) = kvp;

                        var matchingSection = contentSections.Find(section => section.Id == sectionId);

                        if (matchingSection is not null)
                        {
                            matchingSection.Order = newOrder;
                        }
                    });

                    _context.Releases.Update(release);
                    await _context.SaveChangesAsync();
                    return OrderedContentSections(release);
                });
        }

        public Task<Either<ActionResult, ContentSectionViewModel>> AddContentSectionAsync(
            Guid releaseId, ContentSectionAddRequest? request)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId, HydrateContentSectionsAndBlocks)
                .OnSuccess(CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    var orderForNewSection = request?.Order ??
                                             release.GenericContent.Max(contentSection => contentSection.Order) + 1;

                    release.GenericContent
                        .ToList()
                        .FindAll(contentSection => contentSection.Order >= orderForNewSection)
                        .ForEach(contentSection => contentSection.Order++);

                    var newContentSection = new ContentSection
                    {
                        Heading = "New section",
                        Order = orderForNewSection
                    };

                    release.AddGenericContentSection(newContentSection);

                    _context.Releases.Update(release);
                    await _context.SaveChangesAsync();
                    return _mapper.Map<ContentSectionViewModel>(newContentSection);
                });
        }

        public Task<Either<ActionResult, ContentSectionViewModel>> UpdateContentSectionHeading(
            Guid releaseId, Guid contentSectionId, string newHeading)
        {
            return
                CheckContentSectionExists(releaseId, contentSectionId)
                    .OnSuccess(CheckCanUpdateRelease)
                    .OnSuccess(async tuple =>
                    {
                        var (_, sectionToUpdate) = tuple;

                        sectionToUpdate.Heading = newHeading;

                        _context.ContentSections.Update(sectionToUpdate);
                        await _context.SaveChangesAsync();
                        return _mapper.Map<ContentSectionViewModel>(sectionToUpdate);
                    });
        }

        public  Task<Either<ActionResult, List<ContentSectionViewModel>>> RemoveContentSection(
            Guid releaseId,
            Guid contentSectionId)
        {
            return
                CheckContentSectionExists(releaseId, contentSectionId)
                    .OnSuccess(CheckCanUpdateRelease)
                    .OnSuccess(async tuple =>
                    {
                        var (release, sectionToRemove) = tuple;

                        await _contentBlockService.DeleteSectionContentBlocks(sectionToRemove.Id);

                        release.RemoveGenericContentSection(sectionToRemove);
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

        public Task<Either<ActionResult, List<IContentBlockViewModel>>> ReorderContentBlocks(
            Guid releaseId,
            Guid contentSectionId,
            Dictionary<Guid, int> newBlocksOrder)
        {
            return
                CheckContentSectionExists(releaseId, contentSectionId)
                    .OnSuccess(CheckCanUpdateRelease)
                    .OnSuccess(async tuple =>
                    {
                        var (_, section) = tuple;

                        newBlocksOrder.ToList().ForEach(kvp =>
                        {
                            var (blockId, newOrder) = kvp;

                            var matchingBlock = section.Content.Find(block => block.Id == blockId);

                            if (matchingBlock is not null)
                            {
                                matchingBlock.Order = newOrder;
                            }
                        });

                        _context.ContentSections.Update(section);
                        await _context.SaveChangesAsync();
                        return OrderedContentBlocks(section);
                    });
        }

        public Task<Either<ActionResult, IContentBlockViewModel>> AddContentBlock(Guid releaseId,
            Guid contentSectionId,
            ContentBlockAddRequest request)
        {
            if (request.Type != ContentBlockType.HtmlBlock)
            {
                throw new ArgumentOutOfRangeException(nameof(request), "Cannot create type");
            }

            return
                CheckContentSectionExists(releaseId, contentSectionId)
                    .OnSuccess(CheckCanUpdateRelease)
                    .OnSuccess(async tuple =>
                    {
                        var (_, section) = tuple;
                        var newContentBlock = CreateContentBlockForType(request.Type);
                        return await AddContentBlockToContentSectionAndSave(request.Order, section,
                            newContentBlock);
                    });
        }

        public Task<Either<ActionResult, List<IContentBlockViewModel>>> RemoveContentBlock(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId)
        {
            return CheckContentSectionExists(releaseId, contentSectionId)
                .OnSuccess(CheckCanUpdateRelease)
                .OnSuccess(async tuple =>
                {
                    var (_, section) = tuple;

                    var blockToRemove = section.Content.Find(block => block.Id == contentBlockId);

                    if (blockToRemove == null)
                    {
                        return NotFound<List<IContentBlockViewModel>>();
                    }

                    await _contentBlockService.DeleteContentBlockAndReorder(blockToRemove.Id);

                    _context.ContentSections.Update(section);
                    await _context.SaveChangesAsync();
                    return OrderedContentBlocks(section);
                });
        }

        public Task<Either<ActionResult, IContentBlockViewModel>> UpdateTextBasedContentBlock(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, ContentBlockUpdateRequest request)
        {
            return CheckContentSectionExists(releaseId, contentSectionId)
                .OnSuccess(CheckCanUpdateRelease)
                .OnSuccess(async tuple =>
                {
                    var (_, section) = tuple;

                    var blockToUpdate = section.Content.Find(block => block.Id == contentBlockId);

                    if (blockToUpdate == null)
                    {
                        return NotFound<IContentBlockViewModel>();
                    }

                    return blockToUpdate switch
                    {
                        MarkDownBlock markDownBlock => await UpdateMarkDownBlock(markDownBlock, request.Body),
                        HtmlBlock htmlBlock => await UpdateHtmlBlock(htmlBlock, request.Body),
                        DataBlock _ => ValidationActionResult(IncorrectContentBlockTypeForUpdate),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                })
                .OnSuccessDo(block => _hubContext.Clients.Group(releaseId.ToString()).ContentBlockUpdated(block));
        }

        public Task<Either<ActionResult, IContentBlockViewModel>> AttachDataBlock(Guid releaseId, Guid contentSectionId,
            ContentBlockAttachRequest request)
        {
            return CheckContentSectionExists(releaseId, contentSectionId)
                .OnSuccess(CheckCanUpdateRelease)
                .OnSuccess(async tuple =>
                {
                    var (_, section) = tuple;

                    var blockToAttach =
                        _context.ContentBlocks.FirstOrDefault(block => block.Id == request.ContentBlockId);

                    if (blockToAttach == null)
                    {
                        return NotFound<IContentBlockViewModel>();
                    }

                    if (!(blockToAttach is DataBlock dataBlock))
                    {
                        return ValidationActionResult(IncorrectContentBlockTypeForAttach);
                    }

                    if (dataBlock.ContentSectionId.HasValue)
                    {
                        return ValidationActionResult(ContentBlockAlreadyAttachedToContentSection);
                    }

                    return await AddContentBlockToContentSectionAndSave(request.Order, section, dataBlock);
                });
        }

        private async Task<Either<ActionResult, IContentBlockViewModel>> AddContentBlockToContentSectionAndSave(
            int? order, ContentSection section, ContentBlock newContentBlock)
        {
            if (section.Content == null)
            {
                section.Content = new List<ContentBlock>();
            }

            var orderForNewBlock = OrderValueForNewlyAddedContentBlock(order, section);

            section.Content
                .FindAll(contentBlock => contentBlock.Order >= orderForNewBlock)
                .ForEach(contentBlock => contentBlock.Order++);

            newContentBlock.Order = orderForNewBlock;
            section.Content.Add(newContentBlock);

            _context.ContentSections.Update(section);
            await _context.SaveChangesAsync();
            return new Either<ActionResult, IContentBlockViewModel>(_mapper.Map<IContentBlockViewModel>(newContentBlock));
        }

        private static int OrderValueForNewlyAddedContentBlock(int? order, ContentSection section)
        {
            if (order.HasValue)
            {
                return order.Value;
            }

            if (!section.Content.Any())
            {
                return section.Content.Max(contentBlock => contentBlock.Order) + 1;
            }

            return 1;
        }

        private async Task<Either<ActionResult, IContentBlockViewModel>> UpdateMarkDownBlock(MarkDownBlock markDownBlock,
            string body)
        {
            var htmlBlock = new HtmlBlock
            {
                Body = body,
                Comments = markDownBlock.Comments,
                Order = markDownBlock.Order,
                ContentSectionId = markDownBlock.ContentSectionId
            };

            var added = (await _context.ContentBlocks.AddAsync(htmlBlock)).Entity;
            _context.ContentBlocks.Remove(markDownBlock);
            await _context.SaveChangesAsync();
            return _mapper.Map<HtmlBlockViewModel>(added);
        }

        private async Task<Either<ActionResult, IContentBlockViewModel>> UpdateHtmlBlock(HtmlBlock blockToUpdate,
            string body)
        {
            blockToUpdate.Body = body;
            return await SaveContentBlock<HtmlBlockViewModel>(blockToUpdate);
        }

        private async Task<T> SaveContentBlock<T>(ContentBlock blockToUpdate)
            where T: IContentBlockViewModel
        {
            _context.ContentBlocks.Update(blockToUpdate);
            await _context.SaveChangesAsync();
            return _mapper.Map<T>(blockToUpdate);
        }

        private static ContentBlock CreateContentBlockForType(ContentBlockType type)
        {
            var classType = GetContentBlockClassTypeFromEnumValue(type);
            return (ContentBlock) Activator.CreateInstance(classType);
        }

        private List<IContentBlockViewModel> OrderedContentBlocks(ContentSection section)
        {
            return _mapper.Map<List<IContentBlockViewModel>>(section
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

        private static IQueryable<Release> HydrateContentSectionsAndBlocks(IQueryable<Release> releases)
        {
            return releases
                .Include(r => r.Content)
                    .ThenInclude(join => join.ContentSection)
                    .ThenInclude(section => section.Content)
                    .ThenInclude(content => content.Comments)
                    .ThenInclude(comment => comment.CreatedBy)
                .Include(r => r.Content)
                    .ThenInclude(join => join.ContentSection)
                    .ThenInclude(section => section.Content)
                    .ThenInclude(content => content.Comments)
                    .ThenInclude(comment => comment.ResolvedBy)
                .Include(r => r.Content)
                    .ThenInclude(rcs => rcs.ContentSection)
                    .ThenInclude(cs => cs.Content)
                    .ThenInclude(cb => (cb as EmbedBlockLink).EmbedBlock)
                .Include(r => r.Content)
                    .ThenInclude(join => join.ContentSection)
                    .ThenInclude(section => section.Content)
                    .ThenInclude(content => content.LockedBy);
        }

        private Task<Either<ActionResult, Tuple<Release, ContentSection>>> CheckContentSectionExists(
            Guid releaseId, Guid contentSectionId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId, HydrateContentSectionsAndBlocks)
                .OnSuccess(release =>
                {
                    var section = release
                        .Content
                        .Select(join => join.ContentSection)
                        .ToList()
                        .Find(contentSection => contentSection.Id == contentSectionId);

                    if (section == null)
                    {
                        return new NotFoundResult();
                    }

                    return new Either<ActionResult, Tuple<Release, ContentSection>>(
                        TupleOf(release, section));
                });
        }

        private Task<Either<ActionResult, Release>> CheckCanUpdateRelease(Release release)
        {
            return _userService.CheckCanUpdateRelease(release);
        }

        private Task<Either<ActionResult, Tuple<Release, ContentSection>>> CheckCanUpdateRelease(
            Tuple<Release, ContentSection> tuple)
        {
            return _userService
                .CheckCanUpdateRelease(tuple.Item1)
                .OnSuccess(_ => tuple);
        }
    }
}
