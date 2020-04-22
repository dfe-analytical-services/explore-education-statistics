using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataBlockService : IDataBlockService
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IFileStorageService _fileStorageService;

        public DataBlockService(
            ContentDbContext context,
            IMapper mapper, IPersistenceHelper<ContentDbContext> persistenceHelper, 
            IUserService userService,
            IFileStorageService fileStorageService)
        {
            _context = context;
            _mapper = mapper;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _fileStorageService = fileStorageService;
        }

        public async Task<Either<ActionResult, DataBlockViewModel>> CreateAsync(Guid releaseId, CreateDataBlockViewModel createDataBlock)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
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

        public async Task<Either<ActionResult, bool>> DeleteAsync(Guid releaseId, Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<DataBlock>(id)
                .OnSuccess(CheckCanUpdateReleaseForDataBlock)
                .OnSuccess(block => GetDeleteDataBlockPlan(releaseId, id)
                .OnSuccess(async deletePlan =>
                {
                    await DeleteDataBlocks(deletePlan);
                    return true;
                }));
        }
        
        public async Task<Either<ActionResult, bool>> DeleteDataBlocks(DeleteDataBlockPlan deletePlan)
        {
            await DeleteDependentDataBlocks(deletePlan);
            await RemoveChartFileReleaseLinks(deletePlan);
            return true;
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
            return await _persistenceHelper
                .CheckEntityExists<DataBlock>(id)
                .OnSuccess(CheckCanUpdateReleaseForDataBlock)
                .OnSuccess(async existing =>
                {
                    _context.DataBlocks.Update(existing);
                    _mapper.Map(updateDataBlock, existing);
                    await _context.SaveChangesAsync();
                    return await GetAsync(id);
                });
        }

        public async Task<Either<ActionResult, DeleteDataBlockPlan>> GetDeleteDataBlockPlan(Guid releaseId, Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<DataBlock>(id)
                .OnSuccess(CheckCanUpdateReleaseForDataBlock)
                .OnSuccess(block => GetDataBlock(releaseId, block.Id))
                .OnSuccess(block => new DeleteDataBlockPlan
                {
                    ReleaseId = releaseId,
                    DependentDataBlocks = new List<DependentDataBlock>()
                    {
                        CreateDependentDataBlock(block)
                    }
                });
        }
        
        public DeleteDataBlockPlan GetDeleteDataBlockPlan(Guid releaseId, Subject subject)
        {
            return new DeleteDataBlockPlan()
            {
                ReleaseId = releaseId,
                DependentDataBlocks =
                    (subject == null ? new List<DataBlock>() : GetDataBlocks(releaseId, subject.Id)).Select(
                        CreateDependentDataBlock).ToList()
            };
        }

        private DependentDataBlock CreateDependentDataBlock(DataBlock block)
        {
            return new DependentDataBlock()
            {
                Id = block.Id,
                Name = block.Name,
                ContentSectionHeading = GetContentSectionHeading(block),
                InfographicFilenames = block
                    .Charts
                    .Where(chart => chart.Type == ChartType.infographic.ToString())
                    .Cast<InfographicChart>()
                    .Select(chart => chart.FileId)
                    .ToList(),
            };
        }
        
        private string GetContentSectionHeading(DataBlock block)
        {
            var section = block.ContentSection;

            if (section == null)
            {
                return null;
            }

            switch (block.ContentSection.Type)
            {
                case ContentSectionType.Generic: return section.Heading;
                case ContentSectionType.ReleaseSummary: return "Release Summary";
                case ContentSectionType.Headlines: return "Headlines";
                case ContentSectionType.KeyStatistics: return "Key Statistics";
                case ContentSectionType.KeyStatisticsSecondary: return "Key Statistics";
                default: return block.ContentSection.Type.ToString();
            }
        }
        
        private async Task RemoveChartFileReleaseLinks(DeleteDataBlockPlan deletePlan)
        {
            var deletes = deletePlan.DependentDataBlocks.SelectMany(block =>
                block.InfographicFilenames.Select(chartFilename =>
                    _fileStorageService.DeleteNonDataFileAsync(deletePlan.ReleaseId, ReleaseFileTypes.Chart, chartFilename)
                )
            );
            
            await Task.WhenAll(deletes);
        }
        
        

        private async Task RemoveChartFileReleaseLinks(DeleteDataFilePlan deletePlan)
        {
            var deletes = deletePlan.DeleteDataBlockPlan.DependentDataBlocks.SelectMany(block =>
                block.InfographicFilenames.Select(chartFilename =>
                    _fileStorageService.DeleteNonDataFileAsync(deletePlan.ReleaseId, ReleaseFileTypes.Chart, chartFilename)
                )
            );
            
            await Task.WhenAll(deletes);
        }

        private async Task DeleteDependentDataBlocks(DeleteDataBlockPlan deletePlan)
        {
            var blockIdsToDelete = deletePlan
                .DependentDataBlocks
                .Select(block => block.Id);
            
            var dependentDataBlocks = _context
                .DataBlocks
                .Where(block => blockIdsToDelete.Contains(block.Id));
            
            _context.ContentBlocks.RemoveRange(dependentDataBlocks);
            await _context.SaveChangesAsync();
        }

        private List<DataBlock> GetDataBlocks(Guid releaseId, Guid subjectId)
        {
            return _context
                .ReleaseContentBlocks
                .Include(join => join.ContentBlock)
                .ThenInclude(block => block.ContentSection)
                .Where(join => join.ReleaseId == releaseId)
                .ToList()
                .Select(join => join.ContentBlock)
                .Where(block => block.GetType() == typeof(DataBlock))
                .Cast<DataBlock>()
                .Where(block => block.DataBlockRequest.SubjectId == subjectId)
                .ToList();
        }
        
        private DataBlock GetDataBlock(Guid releaseId, Guid id)
        {
            return _context
                .ReleaseContentBlocks
                .Include(join => join.ContentBlock)
                .ThenInclude(block => block.ContentSection)
                .Where(join => join.ReleaseId == releaseId)
                .ToList()
                .Select(join => join.ContentBlock)
                .Where(block => block.GetType() == typeof(DataBlock))
                .Cast<DataBlock>().First(block => block.Id == id);
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
    
    public class DependentDataBlock
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public string? ContentSectionHeading { get; set; }
        
        public List<string> InfographicFilenames { get; set; }
    }
    
    public class DeleteDataBlockPlan
    {
        [JsonIgnore]
        public Guid ReleaseId { get; set; }
        
        public List<DependentDataBlock> DependentDataBlocks { get; set; }
    }
}