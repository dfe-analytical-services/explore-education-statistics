using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using DataBlockId = System.Guid;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataBlockService : IDataBlockService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DataBlockService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<DataBlockViewModel> Get(ReleaseId releaseId, DataBlockId id)
        {
            var release = await _context.Releases.FirstAsync(r => r.Id.Equals(releaseId));
            var dataBlock = GetDataBlocks(release).FirstOrDefault(block => block.Id.Equals(id));
            return _mapper.Map<DataBlockViewModel>(dataBlock);
        }

        public async Task<List<DataBlockViewModel>> ListAsync(ReleaseId releaseId)
        {
            var release = await _context.Releases.FirstAsync(r => r.Id.Equals(releaseId));
            return _mapper.Map<List<DataBlockViewModel>>(GetDataBlocks(release));
        }

        private static IEnumerable<DataBlock> GetDataBlocks(Release release)
        {
            return release.Content.SelectMany(section => section.Content).OfType<DataBlock>();
        }
    }
}