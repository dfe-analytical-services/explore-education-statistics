using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class ContentBlockService : IContentBlockService
    {
        private readonly ContentDbContext _context;

        public ContentBlockService(ContentDbContext context)
        {
            _context = context;
        }

        public async Task DeleteContentBlockAndReorder(Guid blockToRemoveId)
        {
            var blockToRemove = await _context
                .ContentBlocks
                .SingleAsync(cb => cb.Id == blockToRemoveId);

            var originalBlockOrder = blockToRemove.Order;
            var originalContentSectionId = blockToRemove.ContentSectionId;

            await DeleteContentBlock(blockToRemove);

            var contentSection = await _context.ContentSections
                .Include(cs => cs.Content)
                .SingleAsync(cs => cs.Id == originalContentSectionId);

            var contentBlocksRequiringOrderChange = contentSection.Content
                .Where(cb => cb.Order > originalBlockOrder)
                .ToList();

            _context.ContentBlocks.UpdateRange(contentBlocksRequiringOrderChange);
            contentBlocksRequiringOrderChange
                .ForEach(contentBlock => contentBlock.Order--);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteSectionContentBlocks(Guid contentSectionId)
        {
            var contentSection = await _context.ContentSections
                .Include(cs => cs.Content)
                .SingleAsync(cs => cs.Id == contentSectionId);

            await contentSection
                .Content
                .ToList()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async contentBlock =>
                {
                    await DeleteContentBlock(contentBlock);
                });

            await _context.SaveChangesAsync();
        }

        private async Task DeleteContentBlock(ContentBlock blockToRemove)
        {
            switch (blockToRemove)
            {
                case DataBlock dataBlock:
                    // TODO: EES-1306 Refactor data blocks out of content block model
                    dataBlock.Order = 0;
                    dataBlock.ContentSectionId = null;
                    dataBlock.ContentSection = null;
                    _context.ContentBlocks.Update(dataBlock);
                    break;
                case EmbedBlockLink embedBlockLink:
                    await _context.Entry(embedBlockLink)
                        .Reference(ebl => ebl.EmbedBlock)
                        .LoadAsync();
                    _context.EmbedBlocks.Remove(embedBlockLink.EmbedBlock);
                    _context.EmbedBlockLinks.Remove(embedBlockLink);
                    break;
                default:
                    _context.ContentBlocks.Remove(blockToRemove);
                    break;
            }
        }
    }
}
