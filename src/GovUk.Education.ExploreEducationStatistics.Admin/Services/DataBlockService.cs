using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using DataBlockId = System.Guid;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataBlockService : IDataBlockService
    {
        private readonly ApplicationDbContext _context;
        private readonly IReleaseService _releaseService;
        private readonly IMapper _mapper;
        
        public DataBlockService(ApplicationDbContext context,
            IReleaseService releaseService,
            IMapper mapper)
        {
            _context = context;
            _releaseService = releaseService;
            _mapper = mapper;
        }

        public async Task<Either<ValidationResult, DataBlockViewModel>> CreateAsync(DataBlockId releaseId,
            CreateDataBlockViewModel createDataBlock)
        {
            var release = await _releaseService.GetAsync(releaseId);
            var id = DataBlockId.NewGuid();

            var dataBlock = _mapper.Map<DataBlock>(createDataBlock);
            // TODO DFE-1341 update the Content with the data block

            _context.Releases.Update(release);
            await _context.SaveChangesAsync();
            
            // TODO DFE-1341 Remove me once the data block has been inserted
            return new DataBlockViewModel
            {
                Id = id
            };
            
            //return await GetAsync(releaseId, id);
        }

        public async Task<DataBlockViewModel> GetAsync(ReleaseId releaseId, DataBlockId id)
        {
            var release = await _releaseService.GetAsync(releaseId);
            var dataBlock = GetDataBlocks(release).FirstOrDefault(block => block.Id.Equals(id));
            return _mapper.Map<DataBlockViewModel>(dataBlock);
        }

        public async Task<List<DataBlockViewModel>> ListAsync(ReleaseId releaseId)
        {
            var release = await _releaseService.GetAsync(releaseId);
            return _mapper.Map<List<DataBlockViewModel>>(GetDataBlocks(release));
        }

        private static IEnumerable<DataBlock> GetDataBlocks(Release release)
        {
            return release.Content.SelectMany(section => section.Content).OfType<DataBlock>();
        }
    }
}