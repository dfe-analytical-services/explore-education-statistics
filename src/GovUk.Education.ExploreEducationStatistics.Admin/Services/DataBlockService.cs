#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
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
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataBlockService : IDataBlockService
    {
        private readonly ContentDbContext _context;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IReleaseFileService _releaseFileService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public DataBlockService(ContentDbContext context,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IReleaseFileService releaseFileService,
            IUserService userService,
            IMapper mapper)
        {
            _context = context;
            _persistenceHelper = persistenceHelper;
            _releaseFileService = releaseFileService;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, DataBlockViewModel>> Create(Guid releaseId,
            DataBlockCreateViewModel dataBlockCreate)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    var dataBlock = _mapper.Map<DataBlock>(dataBlockCreate);
                    dataBlock.Created = DateTime.UtcNow;
                    dataBlock.ReleaseId = release.Id;

                    var added = (await _context.DataBlocks.AddAsync(dataBlock)).Entity;

                    await _context.SaveChangesAsync();

                    return await Get(added.Id);
                });
        }

        public async Task<Either<ActionResult, Unit>> Delete(Guid releaseId, Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccessDo(_userService.CheckCanUpdateRelease)
                .OnSuccess(release => GetDeletePlan(release.Id, id))
                .OnSuccessVoid(DeleteDataBlocks);
        }

        public async Task DeleteDataBlocks(DeleteDataBlockPlan deletePlan)
        {
            await DeleteDependentDataBlocks(deletePlan);
            await RemoveChartFileReleaseLinks(deletePlan);
        }

        public async Task<Either<ActionResult, Unit>> RemoveChartFile(Guid releaseId, Guid id)
        {
            return await RemoveInfographicChartFromDataBlock(releaseId, id)
                .OnSuccess(async () => await _releaseFileService.Delete(releaseId, id));
        }

        public async Task<Either<ActionResult, DataBlockViewModel>> Get(Guid id)
        {
            return await GetDataBlock(id)
                .OnSuccessDo(dataBlock => _userService.CheckCanViewRelease(dataBlock.Release))
                .OnSuccess(dataBlock => _mapper.Map<DataBlockViewModel>(dataBlock));
        }

        public async Task<Either<ActionResult, List<DataBlockSummaryViewModel>>> List(Guid releaseId)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async release =>
                    {
                        var dataBlocks = await GetDataBlocks(release.Id);
                        return _mapper.Map<List<DataBlockSummaryViewModel>>(dataBlocks)
                            .OrderBy(model => model.Name)
                            .ToList();
                    }
                );
        }

        public async Task<Either<ActionResult, DataBlockViewModel>> Update(
            Guid id,
            DataBlockUpdateViewModel dataBlockUpdate)
        {
            return await GetDataBlock(id)
                .OnSuccessDo(dataBlock => _userService.CheckCanUpdateRelease(dataBlock.Release))
                .OnSuccess(async dataBlock =>
                    {
                        // TODO EES-753 Alter this when multiple charts are supported
                        var infographicChart = dataBlock.Charts.OfType<InfographicChart>().FirstOrDefault();
                        var updatedInfographicChart =
                            dataBlockUpdate.Charts.OfType<InfographicChart>().FirstOrDefault();

                        if (infographicChart != null &&
                            infographicChart.FileId != updatedInfographicChart?.FileId)
                        {
                            await _releaseFileService.Delete(dataBlock.ReleaseId, new Guid(infographicChart.FileId));
                        }

                        _mapper.Map(dataBlockUpdate, dataBlock);

                        _context.DataBlocks.Update(dataBlock);
                        await _context.SaveChangesAsync();

                        return await Get(id);
                    }
                );
        }

        public async Task<Either<ActionResult, DeleteDataBlockPlan>> GetDeletePlan(Guid releaseId, Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<DataBlock>(id, query => query
                    .Include(rcb => rcb.Release))
                .OnSuccessDo(dataBlock => _userService.CheckCanUpdateRelease(dataBlock.Release))
                .OnSuccess(async dataBlock =>
                    new DeleteDataBlockPlan
                    {
                        ReleaseId = releaseId,
                        DependentDataBlocks = new List<DependentDataBlock>
                        {
                            await CreateDependentDataBlock(dataBlock)
                        }
                    }
                );
        }

        public async Task<DeleteDataBlockPlan> GetDeletePlan(Guid releaseId, Subject? subject)
        {
            var blocks = subject == null ? new List<DataBlock>() : await GetDataBlocks(releaseId, subject.Id);
            var dependentBlocks = new List<DependentDataBlock>();
            foreach (var block in blocks)
            {
                dependentBlocks.Add(await CreateDependentDataBlock(block));
            }

            return new DeleteDataBlockPlan
            {
                ReleaseId = releaseId,
                DependentDataBlocks = dependentBlocks
            };
        }

        private async Task<DependentDataBlock> CreateDependentDataBlock(DataBlock block)
        {
            var fileIds = block
                .Charts
                .OfType<InfographicChart>()
                .Select(chart => new Guid(chart.FileId))
                .ToList();

            var files = await _context.Files
                .Where(f => fileIds.Contains(f.Id))
                .ToListAsync();

            return new DependentDataBlock
            {
                Id = block.Id,
                Name = block.Name,
                ContentSectionHeading = await GetContentSectionHeading(block),
                InfographicFilesInfo = files.Select(f => new InfographicFileInfo
                {
                    Id = f.Id,
                    Filename = f.Filename
                }).ToList()
            };
        }

        private async Task<string?> GetContentSectionHeading(DataBlock dataBlock)
        {
            await _context.Entry(dataBlock)
                .Reference(block => block.ContentSection)
                .LoadAsync();

            var section = dataBlock.ContentSection;
            if (section == null)
            {
                return null;
            }

            return section.Type switch
            {
                ReleaseContentSectionType.Generic => section.Heading,
                ReleaseContentSectionType.ReleaseSummary => "Release Summary",
                ReleaseContentSectionType.Headlines => "Headlines",
                ReleaseContentSectionType.KeyStatistics => "Key Statistics",
                ReleaseContentSectionType.KeyStatisticsSecondary => "Key Statistics",
                _ => section.Type.ToString()
            };
        }

        private async Task<Either<ActionResult, Unit>> RemoveInfographicChartFromDataBlock(Guid releaseId, Guid id)
        {
            var blocks = await GetDataBlocks(releaseId);

            foreach (var block in blocks)
            {
                // TODO EES-753 Alter this when multiple charts are supported
                var infoGraphicChart = block.Charts
                    .OfType<InfographicChart>()
                    .FirstOrDefault();

                if (infoGraphicChart != null && infoGraphicChart.FileId == id.ToString())
                {
                    block.Charts.Remove(infoGraphicChart);
                    _context.DataBlocks.Update(block);
                    await _context.SaveChangesAsync();
                    return Unit.Instance;
                }
            }

            return Unit.Instance;
        }

        private async Task RemoveChartFileReleaseLinks(DeleteDataBlockPlan deletePlan)
        {
            var chartFileIds = deletePlan.DependentDataBlocks.SelectMany(
                block => block.InfographicFilesInfo.Select(f => f.Id));

            await _releaseFileService.Delete(deletePlan.ReleaseId, chartFileIds);
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

        private async Task<List<DataBlock>> GetDataBlocks(Guid releaseId, Guid? subjectId = null)
        {
            var dataBlocks = await _context
                .DataBlocks
                .Where(dataBlock => dataBlock.ReleaseId == releaseId)
                .ToListAsync();

            return subjectId == null
                ? dataBlocks
                : dataBlocks.Where(dataBlock => dataBlock.Query.SubjectId == subjectId).ToList();
        }

        private async Task<Either<ActionResult, DataBlock>> GetDataBlock(Guid dataBlockId)
        {
            return await _persistenceHelper.CheckEntityExists<DataBlock>(
                query => query
                    .Include(rcb => rcb.Release)
                    .Where(dataBlock => dataBlock.Id == dataBlockId)
            );
        }
    }

    public class DependentDataBlock
    {
        [JsonIgnore] public Guid Id { get; set; }

        public string Name { get; set; } = "";
        public string? ContentSectionHeading { get; set; }
        public List<InfographicFileInfo> InfographicFilesInfo { get; set; } = new List<InfographicFileInfo>();
    }

    public class InfographicFileInfo
    {
        public Guid Id { get; set; }
        public string Filename { get; set; } = "";
    }

    public class DeleteDataBlockPlan
    {
        [JsonIgnore] public Guid ReleaseId { get; set; }

        public List<DependentDataBlock> DependentDataBlocks { get; set; } = new List<DependentDataBlock>();
    }
}
