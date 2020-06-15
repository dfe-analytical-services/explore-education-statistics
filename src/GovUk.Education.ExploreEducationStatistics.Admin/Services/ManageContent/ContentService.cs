﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ContentBlockUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class ContentService : IContentService
    {
        private readonly IMapper _mapper;
        private readonly ContentDbContext _context;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper; 
        private readonly IUserService _userService; 

        public ContentService(ContentDbContext context,
            IPersistenceHelper<ContentDbContext> persistenceHelper, 
            IMapper mapper, IUserService userService)
        {
            _context = context;
            _persistenceHelper = persistenceHelper;
            _mapper = mapper;
            _userService = userService;
        }

        public Task<Either<ActionResult, List<ContentSectionViewModel>>> GetContentSectionsAsync(
            Guid releaseId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId, HydrateContentSectionsAndBlocks)
                .OnSuccess(OrderedContentSections);
        }

        public Task<Either<ActionResult, List<ContentSectionViewModel>>> ReorderContentSectionsAsync(
            Guid releaseId, 
            Dictionary<Guid, int> newSectionOrder)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId, HydrateContentSectionsAndBlocks)
                .OnSuccess(CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    newSectionOrder.ToList().ForEach(kvp =>
                    {
                        var (sectionId, newOrder) = kvp;
                        release
                            .GenericContent
                            .ToList()
                            .Find(section => section.Id == sectionId).Order = newOrder;
                    });
                    
                    _context.Releases.Update(release);
                    await _context.SaveChangesAsync();
                    return OrderedContentSections(release);
                });
        }

        public Task<Either<ActionResult, ContentSectionViewModel>> AddContentSectionAsync(
            Guid releaseId, AddContentSectionRequest? request)
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

        public Task<Either<ActionResult, ContentSectionViewModel>> UpdateContentSectionHeadingAsync(
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
        
        public  Task<Either<ActionResult, List<ContentSectionViewModel>>> RemoveContentSectionAsync(
            Guid releaseId,
            Guid contentSectionId)
        {
            return 
                CheckContentSectionExists(releaseId, contentSectionId)
                    .OnSuccess(CheckCanUpdateRelease)
                    .OnSuccess(async tuple =>
                    {
                        var (release, sectionToRemove) = tuple;
                        
                        // detach DataBlocks before removing the ContentSection and its ContentBlocks
                        sectionToRemove
                            .Content
                            .FindAll(contentBlock => contentBlock.Type == ContentBlockType.DataBlock.ToString())
                            .ForEach(dataBlock =>  
                                RemoveContentBlockFromContentSection(sectionToRemove, dataBlock, false));
                        
                        release.RemoveGenericContentSection(sectionToRemove);

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

        public Task<Either<ActionResult, ContentSectionViewModel>> GetContentSectionAsync(Guid releaseId, Guid contentSectionId)
        {
            return CheckContentSectionExists(releaseId, contentSectionId)
                    .OnSuccess(tuple => _mapper.Map<ContentSectionViewModel>(tuple.Item2));
        }

        public Task<Either<ActionResult, List<IContentBlock>>> ReorderContentBlocksAsync(Guid releaseId, Guid contentSectionId, Dictionary<Guid, int> newBlocksOrder)
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
                            section.Content.Find(block => block.Id == blockId).Order = newOrder;
                        });
                        
                        _context.ContentSections.Update(section);
                        await _context.SaveChangesAsync();
                        return OrderedContentBlocks(section);
                    });
        }

        public Task<Either<ActionResult, IContentBlock>> AddContentBlockAsync(Guid releaseId, Guid contentSectionId,
            AddContentBlockRequest request)
        {
            if (request.Type == ContentBlockType.DataBlock)
            {
                throw new ArgumentOutOfRangeException(nameof(request), "Cannot create type DataBlock");
            }
            
            return 
                CheckContentSectionExists(releaseId, contentSectionId)
                    .OnSuccess(CheckCanUpdateRelease)
                    .OnSuccess(async tuple =>
                    {
                        var (_, section) = tuple;
                        var newContentBlock = CreateContentBlockForType(request.Type);
                        return await AddContentBlockToContentSectionAndSaveAsync(request.Order, section, newContentBlock);
                    });
        }

        public Task<Either<ActionResult, List<IContentBlock>>> RemoveContentBlockAsync(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId)
        {
            return 
                CheckContentSectionExists(releaseId, contentSectionId)
                    .OnSuccess(CheckCanUpdateRelease)
                    .OnSuccess(async tuple =>
                    {
                        var (_, section) = tuple;

                        var blockToRemove = section.Content.Find(block => block.Id == contentBlockId);

                        if (blockToRemove == null)
                        {
                            return NotFound<List<IContentBlock>>(); 
                        }
                        
                        if (!blockToRemove.ContentSectionId.HasValue)
                        {
                            return ValidationActionResult(ContentBlockAlreadyDetached);
                        }
                        
                        if (blockToRemove.ContentSectionId != contentSectionId)
                        {
                            return ValidationActionResult(ContentBlockNotAttachedToThisContentSection);
                        }

                        var deleteContentBlock = blockToRemove.Type != ContentBlockType.DataBlock.ToString(); 
                        RemoveContentBlockFromContentSection(section, blockToRemove, deleteContentBlock);
                        
                        _context.ContentSections.Update(section);
                        await _context.SaveChangesAsync();
                        return OrderedContentBlocks(section);
                    });
        }

        public Task<Either<ActionResult, IContentBlock>> UpdateDataBlockAsync(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, UpdateDataBlockRequest request)
        {
            return
                CheckContentSectionExists(releaseId, contentSectionId)
                    .OnSuccess(CheckCanUpdateRelease)
                    .OnSuccess(async tuple =>
                    {
                        var (_, section) = tuple;

                        var blockToUpdate = section.Content.Find(block => block.Id == contentBlockId);

                        if (blockToUpdate == null)
                        {
                            return NotFound<IContentBlock>();
                        }

                        switch (Enum.Parse<ContentBlockType>(blockToUpdate.Type))
                        {
                            case ContentBlockType.DataBlock:
                                return await UpdateDataBlock((DataBlock) blockToUpdate, request);
                            default:
                                return ValidationActionResult(IncorrectContentBlockTypeForUpdate);
                        }
                    });
        }


        public Task<Either<ActionResult, IContentBlock>> UpdateTextBasedContentBlockAsync(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, UpdateTextBasedContentBlockRequest request)
        {
            return 
                CheckContentSectionExists(releaseId, contentSectionId)
                    .OnSuccess(CheckCanUpdateRelease)
                    .OnSuccess(async tuple =>
                    {
                        var (_, section) = tuple;

                        var blockToUpdate = section.Content.Find(block => block.Id == contentBlockId);

                        if (blockToUpdate == null)
                        {
                            return NotFound<IContentBlock>();
                        }

                        switch (Enum.Parse<ContentBlockType>(blockToUpdate.Type))
                        {
                            case ContentBlockType.MarkDownBlock:
                                return await UpdateMarkDownBlock((MarkDownBlock) blockToUpdate, request.Body);
                            case ContentBlockType.HtmlBlock:
                                return await UpdateHtmlBlock((HtmlBlock) blockToUpdate, request.Body);
                            case ContentBlockType.DataBlock:
                                return ValidationActionResult(IncorrectContentBlockTypeForUpdate);
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    });
        }

        public async Task<Either<ActionResult, List<T>>> GetUnattachedContentBlocksAsync<T>(Guid releaseId) 
            where T : IContentBlock
        {
            var contentBlockTypeEnum = GetContentBlockTypeEnumValueFromType<T>();
            
            var unattachedContentBlocks = await _context
                .ReleaseContentBlocks
                .Include(join => join.ContentBlock)
                .Where(join => join.ReleaseId == releaseId)
                .Select(join => join.ContentBlock)
                .Where(contentBlock => contentBlock.ContentSectionId == null 
                                       && contentBlock.Type == contentBlockTypeEnum.ToString())
                .OfType<T>()
                .ToListAsync();

            if (typeof(T) == typeof(DataBlock))
            {
                return unattachedContentBlocks
                    .OfType<DataBlock>()
                    .OrderBy(contentBlock => contentBlock.Name)
                    .OfType<T>()
                    .ToList();
            }
            
            return unattachedContentBlocks;
        }

        public Task<Either<ActionResult, IContentBlock>> AttachContentBlockAsync(Guid releaseId, Guid contentSectionId, AttachContentBlockRequest request)
        {
            return 
                CheckContentSectionExists(releaseId, contentSectionId)
                    .OnSuccess(CheckCanUpdateRelease)
                    .OnSuccess(async tuple =>
                    {
                        var (_, section) = tuple;

                        var blockToAttach = _context
                            .ContentBlocks
                            .FirstOrDefault(block => block.Id == request.ContentBlockId);

                        if (blockToAttach == null)
                        {
                            return NotFound<IContentBlock>();
                        }
                        
                        if (blockToAttach.Type != ContentBlockType.DataBlock.ToString())
                        {
                            return ValidationActionResult(IncorrectContentBlockTypeForAttach);
                        }

                        if (blockToAttach.ContentSectionId.HasValue)
                        {
                            return ValidationActionResult(ContentBlockAlreadyAttachedToContentSection);
                        }
                        
                        return await AddContentBlockToContentSectionAndSaveAsync(request.Order, section, blockToAttach);
                    });
        }

        private Task<Either<ActionResult, CommentViewModel>> GetCommentAsync(Guid commentId)
        {
            return _persistenceHelper.CheckEntityExists<Comment>(commentId, queryable => 
                    queryable.Include(comment => comment.CreatedBy))
                .OnSuccess(comment => _mapper.Map<CommentViewModel>(comment));
        }

        public Task<Either<ActionResult, List<CommentViewModel>>> GetCommentsAsync(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId
        )
        {
            return CheckContentSectionExists(releaseId, contentSectionId)
                .OnSuccess(tuple =>
                {
                    var (_, section) = tuple;

                    var contentBlock = section.Content.Find(block => block.Id == contentBlockId);

                    if (contentBlock == null)
                    {
                        return NotFound<List<CommentViewModel>>();
                    }

                    return _mapper.Map<List<CommentViewModel>>(contentBlock.Comments);
                }
            );
        }

        public Task<Either<ActionResult, CommentViewModel>> AddCommentAsync(Guid releaseId,
            Guid contentSectionId,
            Guid contentBlockId,
            AddOrUpdateCommentRequest request)
        {
            return CheckContentSectionExists(releaseId, contentSectionId)
                .OnSuccess(CheckCanUpdateRelease)
                .OnSuccess(async tuple =>
                {
                    var (_, section) = tuple;

                    var contentBlock = section.Content.Find(block => block.Id == contentBlockId);

                    if (contentBlock == null)
                    {
                        return ValidationActionResult<CommentViewModel>(ContentBlockNotFound);
                    }

                    var comment = new Comment
                    {
                        Id = new Guid(),
                        ContentBlockId = contentBlockId,
                        Content = request.Content,
                        Created = DateTime.UtcNow,
                        CreatedById = _userService.GetUserId()
                    };
                    
                    await _context.Comment.AddAsync(comment);
                    await _context.SaveChangesAsync();
                    return await GetCommentAsync(comment.Id);
                }
            );
        }

        public Task<Either<ActionResult, CommentViewModel>> UpdateCommentAsync(Guid commentId,
            AddOrUpdateCommentRequest request)
        {
            return _persistenceHelper.CheckEntityExists<Comment>(commentId)
                .OnSuccess(_userService.CheckCanUpdateComment)
                .OnSuccess(async comment =>
                    {
                        _context.Comment.Update(comment);
                        comment.Content = request.Content;
                        comment.Updated = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        return await GetCommentAsync(commentId);
                    }
                );
        }

        public Task<Either<ActionResult, bool>> DeleteCommentAsync(Guid commentId)
        {
            return _persistenceHelper.CheckEntityExists<Comment>(commentId)
                .OnSuccess(_userService.CheckCanUpdateComment)
                .OnSuccess(async comment =>
                    {
                        _context.Comment.Remove(comment);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                );
        }

        private async Task<Either<ActionResult, IContentBlock>> AddContentBlockToContentSectionAndSaveAsync(int? order, ContentSection section,
            IContentBlock newContentBlock)
        {
            if (section.Content == null)
            {
                section.Content = new List<IContentBlock>();
            }

            // No longer supporting the creation of new MarkDownBlocks
            if (newContentBlock.Type == ContentBlockType.MarkDownBlock.ToString())
            {
                newContentBlock.Type = ContentBlockType.HtmlBlock.ToString();
            }

            var orderForNewBlock = OrderValueForNewlyAddedContentBlock(order, section);

            section.Content
                .FindAll(contentBlock => contentBlock.Order >= orderForNewBlock)
                .ForEach(contentBlock => contentBlock.Order++);

            newContentBlock.Order = orderForNewBlock;
            section.Content.Add(newContentBlock);

            _context.ContentSections.Update(section);
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
            IContentBlock blockToRemove,
            bool deleteContentBlock)
        {
            section.Content.Remove(blockToRemove);

            var removedBlockOrder = blockToRemove.Order;

            section.Content
                .FindAll(contentBlock => contentBlock.Order > removedBlockOrder)
                .ForEach(contentBlock => contentBlock.Order--);

            if (deleteContentBlock)
            {
                _context.ContentBlocks.Remove(blockToRemove);
            }
            else
            {
                blockToRemove.Order = 0;
                blockToRemove.ContentSectionId = null;
                _context.ContentBlocks.Update(blockToRemove);
            }
        }

        private async Task<Either<ActionResult, IContentBlock>> UpdateMarkDownBlock(MarkDownBlock blockToUpdate,
            string body)
        {
            blockToUpdate.Body = body;

            // Convert all MarkDownBlocks to HtmlBlocks
            blockToUpdate.Type = ContentBlockType.HtmlBlock.ToString();

            return await SaveContentBlock(blockToUpdate);
        }

        private async Task<Either<ActionResult, IContentBlock>> UpdateHtmlBlock(HtmlBlock blockToUpdate,
            string body)
        {
            blockToUpdate.Body = body;
            return await SaveContentBlock(blockToUpdate);
        }

        private async Task<Either<ActionResult, IContentBlock>> UpdateDataBlock(DataBlock blockToUpdate,
            UpdateDataBlockRequest request)
        {
            if (blockToUpdate.Summary == null)
            {
                blockToUpdate.Summary = new DataBlockSummary();
            }
            
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

            return await SaveContentBlock(blockToUpdate);
        }

        private async Task<IContentBlock> SaveContentBlock(IContentBlock blockToUpdate)
        {
            _context.ContentBlocks.Update(blockToUpdate);
            await _context.SaveChangesAsync();
            return blockToUpdate;
        }

        private static IContentBlock CreateContentBlockForType(ContentBlockType type)
        {
            var classType = GetContentBlockClassTypeFromEnumValue(type);
            return (IContentBlock) Activator.CreateInstance(classType);
        }

        private static List<IContentBlock> OrderedContentBlocks(ContentSection section)
        {
            return section
                .Content
                .OrderBy(block => block.Order)
                .ToList();
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
                ;
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
                        new Tuple<Release, ContentSection>(release, section));
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