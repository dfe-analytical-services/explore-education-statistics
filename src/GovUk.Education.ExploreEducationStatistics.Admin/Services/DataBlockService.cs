using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataBlockService : IDataBlockService
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPersistenceHelper<Release, Guid> _releaseHelper;
        private readonly IPersistenceHelper<DataBlock, Guid> _dataBlockHelper;
        private readonly IUserService _userService;

        public DataBlockService(
            ContentDbContext context,
            IMapper mapper, IPersistenceHelper<Release, Guid> releaseHelper, 
            IUserService userService, IPersistenceHelper<DataBlock, Guid> dataBlockHelper)
        {
            _context = context;
            _mapper = mapper;
            _releaseHelper = releaseHelper;
            _userService = userService;
            _dataBlockHelper = dataBlockHelper;
        }

        public async Task<Either<ActionResult, DataBlockViewModel>> CreateAsync(Guid releaseId, CreateDataBlockViewModel createDataBlock)
        {
            return await _releaseHelper
                .CheckEntityExistsActionResult(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    var dataBlock = _mapper.Map<DataBlock>(createDataBlock);
                    dataBlock.Id = Guid.NewGuid();

                    await _context.DataBlocks.AddAsync(dataBlock);
            
                    release.AddContentBlock(dataBlock);
                    _context.Releases.Update(release);
            
                    await _context.SaveChangesAsync();

                    return await GetAsync(dataBlock.Id);
                });
        }

        public async Task<Either<ActionResult, bool>> DeleteAsync(Guid id)
        {
            return await _dataBlockHelper
                .CheckEntityExistsActionResult(id)
                .OnSuccess(CheckCanUpdateReleaseForDataBlock)
                .OnSuccess(async dataBlock =>
                {
                    _context.DataBlocks.Remove(dataBlock);
                    await _context.SaveChangesAsync();
                    return true;
                });
        }
        
        public async Task<DataBlockViewModel> GetAsync(Guid id)
        {
            var dataBlock = await _context.DataBlocks.FirstOrDefaultAsync(block => block.Id.Equals(id));
            return _mapper.Map<DataBlockViewModel>(dataBlock);
        }

        public async Task<List<DataBlockViewModel>> ListAsync(Guid releaseId)
        {
            var dataBlocks = await _context
                .ReleaseContentBlocks
                .Where(join => join.ReleaseId == releaseId 
                               && join.ContentBlock.Type == ContentBlockType.DataBlock.ToString())
                .Select(join => join.ContentBlock)
                .OrderBy(dataBlock => ((DataBlock)dataBlock).Name)
                .ToListAsync();
            
            return _mapper.Map<List<DataBlockViewModel>>(dataBlocks);
        }

        public async Task<Either<ActionResult, DataBlockViewModel>> UpdateAsync(Guid id, UpdateDataBlockViewModel updateDataBlock)
        {
            return await _dataBlockHelper
                .CheckEntityExistsActionResult(id)
                .OnSuccess(CheckCanUpdateReleaseForDataBlock)
                .OnSuccess(async existing =>
                {
                    _context.DataBlocks.Update(existing);
                    _mapper.Map(updateDataBlock, existing);
                    await _context.SaveChangesAsync();
                    return await GetAsync(id);
                });
        }

        private async Task<Either<ActionResult, DataBlock>> CheckCanUpdateReleaseForDataBlock(DataBlock dataBlock)
        {
            var release = _context
                .ReleaseContentBlocks
                .Include(join => join.Release)
                .First(join => join.ContentBlockId == dataBlock.Id)
                .Release;

            return await _userService
                .CheckCanUpdateRelease(release)
                .OnSuccess(_ => dataBlock);
        }
    }
}