﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class ContentService : IContentService
    {
        private readonly ContentDbContext _context;
        private readonly PersistenceHelper<Release, Guid> _releaseHelper; 

        public ContentService(ContentDbContext context)
        {
            _context = context;
            _releaseHelper = new PersistenceHelper<Release, Guid>(
                _context, 
                context.Releases, 
                ValidationErrorMessages.ReleaseNotFound);
        }

        public Task<Either<ValidationResult, List<ContentSectionViewModel>>> GetContentSectionsAsync(
            Guid releaseId)
        {
            return _releaseHelper.CheckEntityExists(releaseId, release => 
                release
                    .GenericContent
                    .Select(ContentSectionViewModel.ToViewModel)
                    .OrderBy(c => c.Order)
                    .ToList(),
                HydrateContentSectionsAndBlocks);
        }

        public Task<Either<ValidationResult, List<ContentSectionViewModel>>> ReorderContentSectionsAsync(
            Guid releaseId, 
            Dictionary<Guid, int> newSectionOrder)
        {
            return _releaseHelper.CheckEntityExists(releaseId, async release =>
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
            }, HydrateContentSectionsAndBlocks);
        }

        public Task<Either<ValidationResult, ContentSectionViewModel>> AddContentSectionAsync(
            Guid releaseId, AddContentSectionRequest? request)
        {
            return _releaseHelper.CheckEntityExists(releaseId, async release =>
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
                return ContentSectionViewModel.ToViewModel(newContentSection);
            }, HydrateContentSectionsAndBlocks);
        }

        public Task<Either<ValidationResult, ContentSectionViewModel>> UpdateContentSectionHeadingAsync(
            Guid releaseId, Guid contentSectionId, string newHeading)
        {
            return CheckContentSectionExists(releaseId, contentSectionId, async tuple =>
            {
                var (_, sectionToUpdate) = tuple;
                
                sectionToUpdate.Heading = newHeading;

                _context.ContentSections.Update(sectionToUpdate);
                await _context.SaveChangesAsync();
                return ContentSectionViewModel.ToViewModel(sectionToUpdate);
            });
        }
        
        public Task<Either<ValidationResult, List<ContentSectionViewModel>>> RemoveContentSectionAsync(
            Guid releaseId,
            Guid contentSectionId)
        {
            return CheckContentSectionExists(releaseId, contentSectionId, async tuple =>
            {
                var (release, sectionToRemove) = tuple;
                
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

        public Task<Either<ValidationResult, ContentSectionViewModel>> GetContentSectionAsync(Guid releaseId, Guid contentSectionId)
        {
            return CheckContentSectionExists(releaseId, contentSectionId, 
                tuple => ContentSectionViewModel.ToViewModel(tuple.Item2));
        }

        public Task<Either<ValidationResult, List<IContentBlock>>> ReorderContentBlocksAsync(Guid releaseId, Guid contentSectionId, Dictionary<Guid, int> newBlocksOrder)
        {
            return CheckContentSectionExists(releaseId, contentSectionId, async tuple =>
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

        public Task<Either<ValidationResult, IContentBlock>> AddContentBlockAsync(Guid releaseId, Guid contentSectionId,
            AddContentBlockRequest request) 
        {
            return CheckContentSectionExists(releaseId, contentSectionId, async tuple =>
            {
                var (_, section) = tuple;
                
                var orderForNewBlock = request.Order ?? section.Content.Max(contentBlock => contentBlock.Order) + 1;

                section.Content
                    .FindAll(contentBlock => contentBlock.Order >= orderForNewBlock)
                    .ForEach(contentBlock => contentBlock.Order++);
                    
                IContentBlock newContentBlock = CreateContentBlockForType(request.Type);
                newContentBlock.Order = orderForNewBlock;
                section.Content.Add(newContentBlock);
                
                _context.ContentSections.Update(section);
                await _context.SaveChangesAsync();
                return newContentBlock;
            });
        }

        public Task<Either<ValidationResult, List<IContentBlock>>> RemoveContentBlockAsync(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId)
        {
            return CheckContentSectionExists(releaseId, contentSectionId, async tuple =>
            {
                var (_, section) = tuple;

                var blockToRemove = section.Content.Find(block => block.Id == contentBlockId);

                if (blockToRemove == null)
                {
                    return new Either<ValidationResult, List<IContentBlock>>(
                        ValidationUtils.ValidationResult(ValidationErrorMessages.ContentBlockNotFound)); 
                }
                
                section.Content.Remove(blockToRemove);

                var removedBlockOrder = blockToRemove.Order;

                section.Content
                    .FindAll(contentBlock => contentBlock.Order > removedBlockOrder)
                    .ForEach(contentBlock => contentBlock.Order--);
                
                _context.ContentBlocks.Remove(blockToRemove);
                _context.ContentSections.Update(section);
                await _context.SaveChangesAsync();
                return OrderedContentBlocks(section);
            });
        }

        public Task<Either<ValidationResult, IContentBlock>> UpdateTextBasedContentBlockAsync(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, UpdateTextBasedContentBlockRequest request)
        {
            return CheckContentSectionExists(releaseId, contentSectionId, async tuple =>
            {
                var (_, section) = tuple;

                var blockToUpdate = section.Content.Find(block => block.Id == contentBlockId);

                if (blockToUpdate == null)
                {
                    return new Either<ValidationResult, IContentBlock>(
                        ValidationUtils.ValidationResult(ValidationErrorMessages.ContentBlockNotFound));
                }

                switch (Enum.Parse<ContentBlockType>(blockToUpdate.Type))
                {
                    case ContentBlockType.MarkDownBlock:
                        return await UpdateMarkDownBlock((MarkDownBlock) blockToUpdate, request.Body);
                    case ContentBlockType.HtmlBlock:
                        return await UpdateHtmlBlock((HtmlBlock) blockToUpdate, request.Body);
                    case ContentBlockType.InsetTextBlock:
                        return await UpdateInsetTextBlock((InsetTextBlock) blockToUpdate, request.Heading, request.Body);
                    case ContentBlockType.DataBlock:
                        return new Either<ValidationResult, IContentBlock>(
                            ValidationUtils.ValidationResult(ValidationErrorMessages.IncorrectContentBlockTypeForUpdate));
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        }

        private async Task<Either<ValidationResult, IContentBlock>> UpdateMarkDownBlock(MarkDownBlock blockToUpdate, string body)
        {
            blockToUpdate.Body = body;
            return await SaveContentBlock(blockToUpdate);
        }

        private async Task<Either<ValidationResult, IContentBlock>> UpdateHtmlBlock(HtmlBlock blockToUpdate, string body)
        {
            blockToUpdate.Body = body;
            return await SaveContentBlock(blockToUpdate);
        }

        private async Task<Either<ValidationResult, IContentBlock>> UpdateInsetTextBlock(InsetTextBlock blockToUpdate, string heading, string body)
        {
            blockToUpdate.Heading = heading;
            blockToUpdate.Body = body;
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
            return type switch
            {
                ContentBlockType.MarkDownBlock => (IContentBlock) new MarkDownBlock(),
                ContentBlockType.InsetTextBlock => new InsetTextBlock(),
                ContentBlockType.HtmlBlock => new HtmlBlock(),
                ContentBlockType.DataBlock => new DataBlock(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        private static List<IContentBlock> OrderedContentBlocks(ContentSection section)
        {
            return section
                .Content
                .OrderBy(block => block.Order)
                .ToList();
        }

        private static List<ContentSectionViewModel> OrderedContentSections(Release release)
        {
            return release
                .GenericContent
                .Select(ContentSectionViewModel.ToViewModel)
                .OrderBy(c => c.Order)
                .ToList();
        }

        private static IQueryable<Release> HydrateContentSectionsAndBlocks(IQueryable<Release> releases)
        {
            return releases
                .Include(r => r.Content)
                .ThenInclude(join => join.ContentSection)
                .ThenInclude(section => section.Content);
        }
        
        private Task<Either<ValidationResult, T>> CheckContentSectionExists<T>(
            Guid releaseId, Guid contentSectionId, 
            Func<Tuple<Release, ContentSection>, Task<T>> contentSectionFn)
        {
            Task<Either<ValidationResult, T>> ContentSectionEitherFn(Tuple<Release, ContentSection> tuple)
            {
                return contentSectionFn
                    .Invoke(tuple)
                    .ContinueWith(result => new Either<ValidationResult, T>(result.Result));
            }
            
            return CheckContentSectionExists(releaseId, contentSectionId, ContentSectionEitherFn);
        }

        private Task<Either<ValidationResult, T>> CheckContentSectionExists<T>(
            Guid releaseId, Guid contentSectionId, 
            Func<Tuple<Release, ContentSection>, Either<ValidationResult, T>> contentSectionFn)
        {
            Task<Either<ValidationResult, T>> ContentSectionTaskFn(
                Tuple<Release, ContentSection> tuple)
            {
                var result = contentSectionFn.Invoke(tuple);
                return Task.FromResult(result);
            }

            return CheckContentSectionExists(releaseId, contentSectionId, ContentSectionTaskFn);
        }

        private Task<Either<ValidationResult, T>> CheckContentSectionExists<T>(
            Guid releaseId, Guid contentSectionId, 
            Func<Tuple<Release, ContentSection>, T> contentSectionFn)
        {
            Either<ValidationResult, T> ContentSectionTaskFn(
                Tuple<Release, ContentSection> tuple)
            {
                return new Either<ValidationResult, T>(contentSectionFn.Invoke(tuple));
            }

            return CheckContentSectionExists(releaseId, contentSectionId, ContentSectionTaskFn);
        }

        private Task<Either<ValidationResult, T>> CheckContentSectionExists<T>(
            Guid releaseId, Guid contentSectionId, 
            Func<Tuple<Release, ContentSection>, Task<Either<ValidationResult, T>>> contentSectionFn)
        {
            return _releaseHelper.CheckEntityExists(releaseId, async release =>
            {
                var section = release
                    .Content
                    .Select(join => join.ContentSection)
                    .ToList()
                    .Find(join => join.Id == contentSectionId);

                if (section == null)
                {
                    return new Either<ValidationResult, T>(
                        ValidationUtils.ValidationResult(ValidationErrorMessages.ContentSectionNotFound));
                }

                return await contentSectionFn.Invoke(new Tuple<Release, ContentSection>(release, section));
            }, HydrateContentSectionsAndBlocks);
        }
    }
}