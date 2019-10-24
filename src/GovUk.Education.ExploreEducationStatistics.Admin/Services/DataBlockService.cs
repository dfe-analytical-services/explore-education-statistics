using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using ContentSectionId = System.Guid;
using DataBlockId = System.Guid;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataBlockService : IDataBlockService
    {
        private readonly ContentDbContext _context;
        private readonly IReleaseService _releaseService;
        private readonly IMapper _mapper;

        public DataBlockService(
            ContentDbContext context,
            IReleaseService releaseService,
            IMapper mapper)
        {
            _context = context;
            _releaseService = releaseService;
            _mapper = mapper;
        }

        public async Task<DataBlockViewModel> CreateAsync(ReleaseId releaseId, CreateDataBlockViewModel createDataBlock)
        {
            var dataBlock = _mapper.Map<DataBlock>(createDataBlock);
            dataBlock.Id = DataBlockId.NewGuid();

            await _context.DataBlocks.AddAsync(dataBlock);
            await _context.SaveChangesAsync();

            return await GetAsync(dataBlock.Id);
        }

        public async Task DeleteAsync(DataBlockId id)
        {
            var dataBlock = await _context.DataBlocks.FirstOrDefaultAsync(block => block.Id.Equals(id));
            if (dataBlock != null)
            {
                _context.DataBlocks.Remove(dataBlock);
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task<DataBlockViewModel> GetAsync(DataBlockId id)
        {
            var dataBlock = await _context.DataBlocks.FirstOrDefaultAsync(block => block.Id.Equals(id));
            return _mapper.Map<DataBlockViewModel>(dataBlock);
        }

        public async Task<List<DataBlockViewModel>> ListAsync(ReleaseId releaseId)
        {
            var release = await _releaseService.GetAsync(releaseId);
            
            var dataBlocks = await _context.DataBlocks.Where(block => 
                block.ContentSection.ReleaseId.Equals(releaseId) || 
                block.Id.Equals(release.KeyStatisticsId)).ToListAsync();
            
            return _mapper.Map<List<DataBlockViewModel>>(dataBlocks);
        }

        public async Task<DataBlockViewModel> UpdateAsync(DataBlockId id, UpdateDataBlockViewModel updateDataBlock)
        {
            var existing = await _context.DataBlocks.FirstOrDefaultAsync(block => block.Id.Equals(id));
            _context.DataBlocks.Update(existing);
            _mapper.Map(updateDataBlock, existing);
            await _context.SaveChangesAsync();
            return await GetAsync(id);
        }
    }
}