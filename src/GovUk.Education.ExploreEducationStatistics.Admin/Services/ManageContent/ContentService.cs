using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs.Clients;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ContentBlockUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;

public class ContentService : IContentService
{
    private readonly ContentDbContext _context;
    private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
    private readonly IContentSectionRepository _contentSectionRepository;
    private readonly IContentBlockService _contentBlockService;
    private readonly IHubContext<ReleaseContentHub, IReleaseContentHubClient> _hubContext;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public ContentService(ContentDbContext context,
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IContentSectionRepository contentSectionRepository,
        IContentBlockService contentBlockService,
        IHubContext<ReleaseContentHub, IReleaseContentHubClient> hubContext,
        IUserService userService,
        IMapper mapper)
    {
        _context = context;
        _persistenceHelper = persistenceHelper;
        _contentSectionRepository = contentSectionRepository;
        _contentBlockService = contentBlockService;
        _hubContext = hubContext;
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<Either<ActionResult, List<T>>> GetContentBlocks<T>(Guid releaseVersionId)
        where T : ContentBlock
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccess(releaseVersion => _contentSectionRepository.GetAllContentBlocks<T>(releaseVersion.Id));
    }

    public Task<Either<ActionResult, List<ContentSectionViewModel>>> ReorderContentSections(
        Guid releaseVersionId,
        Dictionary<Guid, int> newSectionOrder)
    {
        return _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, HydrateContentSectionsAndBlocks)
            .OnSuccess(CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                var contentSections = releaseVersion
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

                _context.ReleaseVersions.Update(releaseVersion);
                await _context.SaveChangesAsync();
                return OrderedContentSections(releaseVersion);
            });
    }

    public Task<Either<ActionResult, ContentSectionViewModel>> AddContentSectionAsync(
        Guid releaseVersionId, ContentSectionAddRequest? request)
    {
        return _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, HydrateContentSectionsAndBlocks)
            .OnSuccess(CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                var orderForNewSection = request?.Order ??
                                         releaseVersion.GenericContent.Max(contentSection => contentSection.Order) + 1;

                releaseVersion.GenericContent
                    .ToList()
                    .FindAll(contentSection => contentSection.Order >= orderForNewSection)
                    .ForEach(contentSection => contentSection.Order++);

                var newContentSection = new ContentSection
                {
                    Heading = "New section",
                    Order = orderForNewSection
                };

                releaseVersion.Content.Add(newContentSection);

                _context.ReleaseVersions.Update(releaseVersion);
                await _context.SaveChangesAsync();
                return _mapper.Map<ContentSectionViewModel>(newContentSection);
            });
    }

    public Task<Either<ActionResult, ContentSectionViewModel>> UpdateContentSectionHeading(
        Guid releaseVersionId, Guid contentSectionId, string newHeading)
    {
        return
            CheckContentSectionExists(releaseVersionId: releaseVersionId,
                    contentSectionId: contentSectionId)
                .OnSuccess(CheckCanUpdateReleaseVersion)
                .OnSuccess(async tuple =>
                {
                    var (_, sectionToUpdate) = tuple;

                    sectionToUpdate.Heading = newHeading;

                    _context.ContentSections.Update(sectionToUpdate);
                    await _context.SaveChangesAsync();
                    return _mapper.Map<ContentSectionViewModel>(sectionToUpdate);
                });
    }

    public Task<Either<ActionResult, List<ContentSectionViewModel>>> RemoveContentSection(
        Guid releaseVersionId,
        Guid contentSectionId)
    {
        return
            CheckContentSectionExists(releaseVersionId: releaseVersionId,
                    contentSectionId: contentSectionId)
                .OnSuccess(CheckCanUpdateReleaseVersion)
                .OnSuccess(async tuple =>
                {
                    var (releaseVersion, sectionToRemove) = tuple;

                    await _contentBlockService.DeleteSectionContentBlocks(sectionToRemove.Id);

                    releaseVersion.Content.Remove(sectionToRemove);
                    _context.ContentSections.Remove(sectionToRemove);

                    var removedSectionOrder = sectionToRemove.Order;

                    releaseVersion.GenericContent
                        .ToList()
                        .FindAll(contentSection => contentSection.Order > removedSectionOrder)
                        .ForEach(contentSection => contentSection.Order--);

                    _context.ReleaseVersions.Update(releaseVersion);
                    await _context.SaveChangesAsync();
                    return OrderedContentSections(releaseVersion);
                });
    }

    public Task<Either<ActionResult, List<IContentBlockViewModel>>> ReorderContentBlocks(
        Guid releaseVersionId,
        Guid contentSectionId,
        Dictionary<Guid, int> newBlocksOrder)
    {
        return
            CheckContentSectionExists(releaseVersionId: releaseVersionId,
                    contentSectionId: contentSectionId)
                .OnSuccess(CheckCanUpdateReleaseVersion)
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

    public Task<Either<ActionResult, IContentBlockViewModel>> AddContentBlock(Guid releaseVersionId,
        Guid contentSectionId,
        ContentBlockAddRequest request)
    {
        if (request.Type != ContentBlockType.HtmlBlock)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Cannot create type");
        }

        return
            CheckContentSectionExists(releaseVersionId: releaseVersionId,
                    contentSectionId: contentSectionId)
                .OnSuccess(CheckCanUpdateReleaseVersion)
                .OnSuccess(async tuple =>
                {
                    var (_, section) = tuple;
                    var newContentBlock = CreateContentBlockForType(request.Type);
                    return await AddContentBlockToContentSectionAndSave(request.Order, section,
                        newContentBlock);
                });
    }

    public Task<Either<ActionResult, List<IContentBlockViewModel>>> RemoveContentBlock(
        Guid releaseVersionId, Guid contentSectionId, Guid contentBlockId)
    {
        return CheckContentSectionExists(releaseVersionId: releaseVersionId,
                contentSectionId: contentSectionId)
            .OnSuccess(CheckCanUpdateReleaseVersion)
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

                var comments = _context.Comment
                    .Where(c => c.ContentBlockId == blockToRemove.Id);
                _context.RemoveRange(comments);

                await _context.SaveChangesAsync();
                return OrderedContentBlocks(section);
            });
    }

    public Task<Either<ActionResult, IContentBlockViewModel>> UpdateTextBasedContentBlock(
        Guid releaseVersionId, Guid contentSectionId, Guid contentBlockId, ContentBlockUpdateRequest request)
    {
        return CheckContentSectionExists(releaseVersionId: releaseVersionId,
                contentSectionId: contentSectionId)
            .OnSuccess(CheckCanUpdateReleaseVersion)
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
                    HtmlBlock htmlBlock => await UpdateHtmlBlock(htmlBlock, request.Body),
                    DataBlock _ => ValidationActionResult(IncorrectContentBlockTypeForUpdate),
                    _ => throw new ArgumentOutOfRangeException()
                };
            })
            .OnSuccessDo(block => _hubContext.Clients.Group(releaseVersionId.ToString()).ContentBlockUpdated(block));
    }

    public Task<Either<ActionResult, DataBlockViewModel>> AttachDataBlock(Guid releaseVersionId,
        Guid contentSectionId,
        DataBlockAttachRequest request)
    {
        return CheckContentSectionExists(releaseVersionId: releaseVersionId,
                contentSectionId: contentSectionId)
            .OnSuccess(CheckCanUpdateReleaseVersion)
            .OnSuccess(async tuple =>
            {
                var (_, section) = tuple;

                var dataBlockVersion = _context
                    .DataBlockVersions
                    .FirstOrDefault(block => block.Id == request.ContentBlockId);

                if (dataBlockVersion == null)
                {
                    return NotFound<DataBlockViewModel>();
                }

                if (dataBlockVersion.ContentSectionId.HasValue)
                {
                    return ValidationActionResult(ContentBlockAlreadyAttachedToContentSection);
                }

                return await AddContentBlockToContentSectionAndSave(
                        request.Order, section, dataBlockVersion.ContentBlock)
                    .OnSuccess(contentBlockViewModel => contentBlockViewModel as DataBlockViewModel);
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
        newContentBlock.ReleaseVersionId = section.ReleaseVersionId;
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

    private List<ContentSectionViewModel> OrderedContentSections(ReleaseVersion releaseVersion)
    {
        return _mapper.Map<List<ContentSectionViewModel>>(releaseVersion.GenericContent)
            .OrderBy(c => c.Order)
            .ToList();
    }

    private static IQueryable<ReleaseVersion> HydrateContentSectionsAndBlocks(
        IQueryable<ReleaseVersion> releaseVersions)
    {
        return releaseVersions
            .Include(rv => rv.Content)
            .ThenInclude(section => section.Content)
            .ThenInclude(block => block.Comments)
            .ThenInclude(comment => comment.CreatedBy)
            .Include(rv => rv.Content)
            .ThenInclude(section => section.Content)
            .ThenInclude(block => block.Comments)
            .ThenInclude(comment => comment.ResolvedBy)
            .Include(rv => rv.Content)
            .ThenInclude(section => section.Content)
            .ThenInclude(block => (block as EmbedBlockLink).EmbedBlock)
            .Include(rv => rv.Content)
            .ThenInclude(section => section.Content)
            .ThenInclude(block => block.LockedBy);
    }

    private Task<Either<ActionResult, Tuple<ReleaseVersion, ContentSection>>> CheckContentSectionExists(
        Guid releaseVersionId,
        Guid contentSectionId)
    {
        return _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, HydrateContentSectionsAndBlocks)
            .OnSuccessCombineWith(releaseVersion => releaseVersion
                .Content
                .FirstOrDefault(contentSection => contentSection.Id == contentSectionId)
                .OrNotFound());
    }

    private Task<Either<ActionResult, ReleaseVersion>> CheckCanUpdateReleaseVersion(ReleaseVersion releaseVersion)
    {
        return _userService.CheckCanUpdateReleaseVersion(releaseVersion);
    }

    private Task<Either<ActionResult, Tuple<ReleaseVersion, ContentSection>>> CheckCanUpdateReleaseVersion(
        Tuple<ReleaseVersion, ContentSection> tuple)
    {
        return _userService
            .CheckCanUpdateReleaseVersion(tuple.Item1)
            .OnSuccess(_ => tuple);
    }
}
