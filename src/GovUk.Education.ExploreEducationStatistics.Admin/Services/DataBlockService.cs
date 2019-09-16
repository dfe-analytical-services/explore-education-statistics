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
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DataBlockService(ApplicationDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<DataBlockViewModel> CreateAsync(ReleaseId releaseId, CreateDataBlockViewModel createDataBlock)
        {
            var dataBlock = _mapper.Map<DataBlock>(createDataBlock);
            dataBlock.Id = DataBlockId.NewGuid();
            dataBlock.ReleaseId = releaseId;

            await _context.DataBlocks.AddAsync(dataBlock);
            await _context.SaveChangesAsync();

            return await GetAsync(dataBlock.Id);
        }

        public async Task<DataBlockViewModel> GetAsync(DataBlockId id)
        {
            var dataBlock = await _context.DataBlocks.FirstOrDefaultAsync(block => block.Id.Equals(id));
            return _mapper.Map<DataBlockViewModel>(dataBlock);
        }

        public async Task<List<DataBlockViewModel>> ListAsync(ReleaseId releaseId)
        {
            var releases = await _context.DataBlocks.Where(block => block.ReleaseId.Equals(releaseId)).ToListAsync();
            return _mapper.Map<List<DataBlockViewModel>>(releases);
        }
    }
}