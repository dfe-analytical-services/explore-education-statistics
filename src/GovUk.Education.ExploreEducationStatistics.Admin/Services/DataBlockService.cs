#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
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
        private readonly IReleaseContentBlockRepository _releaseContentBlockRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IBlobCacheService _cacheService;
        private readonly ICacheKeyService _cacheKeyService;

        public DataBlockService(ContentDbContext context,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IReleaseFileService releaseFileService,
            IReleaseContentBlockRepository releaseContentBlockRepository,
            IUserService userService,
            IMapper mapper,
            IBlobCacheService cacheService,
            ICacheKeyService cacheKeyService)
        {
            _context = context;
            _persistenceHelper = persistenceHelper;
            _releaseFileService = releaseFileService;
            _releaseContentBlockRepository = releaseContentBlockRepository;
            _userService = userService;
            _mapper = mapper;
            _cacheService = cacheService;
            _cacheKeyService = cacheKeyService;
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

                    var added = (await _context.DataBlocks.AddAsync(dataBlock)).Entity;

                    release.AddContentBlock(added);
                    _context.Releases.Update(release);

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

        public Task<Either<ActionResult, Unit>> DeleteDataBlocks(DeleteDataBlockPlan deletePlan)
        {
            return InvalidateDataBlockCaches(deletePlan)
                .OnSuccessVoid(async () =>
                {
                    var dataBlockIds = deletePlan.DependentDataBlocks
                        .Select(db => db.Id)
                        .ToList();
                    var keyStats = _context.KeyStatisticsDataBlock
                        .Where(ks => dataBlockIds.Contains(ks.DataBlockId));
                    _context.KeyStatisticsDataBlock.RemoveRange(keyStats);
                    await _context.SaveChangesAsync();

                    await RemoveChartFileReleaseLinks(deletePlan);
                })
                .OnSuccess(() => DeleteDependentDataBlocks(deletePlan));
        }

        public async Task<Either<ActionResult, Unit>> RemoveChartFile(Guid releaseId, Guid id)
        {
            return await RemoveInfographicChartFromDataBlock(releaseId, id)
                .OnSuccess(async () => await _releaseFileService.Delete(releaseId, id));
        }

        public async Task<Either<ActionResult, DataBlockViewModel>> Get(Guid id)
        {
            return await GetReleaseContentBlock(id)
                .OnSuccessDo(rcb => _userService.CheckCanViewRelease(rcb.Release))
                .OnSuccess(CheckIsDataBlock)
                .OnSuccess(dataBlock => _mapper.Map<DataBlockViewModel>(dataBlock));
        }

        public async Task<Either<ActionResult, List<DataBlockSummaryViewModel>>> List(Guid releaseId)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async release =>
                    {
                        var dataBlocks = await _releaseContentBlockRepository.GetAll<DataBlock>(release.Id);

                        var dataBlockIdsAttachedToKeyStats = await _context.KeyStatisticsDataBlock
                            .Where(ks => ks.ReleaseId == release.Id)
                            .Select(ks => ks.DataBlockId)
                            .ToListAsync();
                        return dataBlocks.Select(block =>
                            {
                                var inContent = block.ContentSectionId != null
                                                || dataBlockIdsAttachedToKeyStats.Contains(block.Id);
                                return new DataBlockSummaryViewModel
                                {
                                    Id = block.Id,
                                    Heading = block.Heading,
                                    Name = block.Name,
                                    Created = block.Created,
                                    HighlightName = block.HighlightName,
                                    HighlightDescription = block.HighlightDescription,
                                    Source = block.Source,
                                    ChartsCount = block.Charts.Count,
                                    InContent = inContent,
                                };
                            })
                            .OrderBy(model => model.Name)
                            .ToList();
                    }
                );
        }

        public async Task<Either<ActionResult, DataBlockViewModel>> Update(
            Guid id,
            DataBlockUpdateViewModel dataBlockUpdate)
        {
            return await
                GetReleaseContentBlock(id)
                    .OnSuccessDo(rcb => _userService.CheckCanUpdateRelease(rcb.Release))
                    .OnSuccess(rcb => CheckIsDataBlock(rcb)
                        .OnSuccessDo(async dataBlock =>
                        {
                            // TODO EES-753 Alter this when multiple charts are supported
                            var infographicChart = dataBlock.Charts.OfType<InfographicChart>().FirstOrDefault();
                            var updatedInfographicChart =
                                dataBlockUpdate.Charts.OfType<InfographicChart>().FirstOrDefault();

                            if (infographicChart != null &&
                                infographicChart.FileId != updatedInfographicChart?.FileId)
                            {
                                await _releaseFileService.Delete(rcb.ReleaseId, new Guid(infographicChart.FileId));
                            }

                            _mapper.Map(dataBlockUpdate, dataBlock);

                            _context.DataBlocks.Update(dataBlock);
                            await _context.SaveChangesAsync();
                        })
                        .OnSuccessDo(() => InvalidateCachedDataBlock(rcb.ReleaseId, rcb.ContentBlockId)))
                    .OnSuccess(() => Get(id));
        }

        public async Task<Either<ActionResult, DeleteDataBlockPlan>> GetDeletePlan(Guid releaseId, Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseContentBlock>(
                    query => query
                        .Include(rcb => rcb.Release)
                        .Include(rcb => rcb.ContentBlock)
                        .ThenInclude(block => block.ContentSection)
                        .Where(rcb => rcb.ReleaseId == releaseId && rcb.ContentBlockId == id)
                )
                .OnSuccessDo(rcb => _userService.CheckCanUpdateRelease(rcb.Release))
                .OnSuccess(
                    rcb => CheckIsDataBlock(rcb)
                        .OnSuccess(
                            async dataBlock =>
                                new DeleteDataBlockPlan
                                {
                                    ReleaseId = releaseId,
                                    DependentDataBlocks = new List<DependentDataBlock>()
                                    {
                                        await CreateDependentDataBlock(dataBlock)
                                    },
                                }
                        )
                );
        }

        public async Task<DeleteDataBlockPlan> GetDeletePlan(Guid releaseId, Subject? subject)
        {
            var blocks = subject == null ? new List<DataBlock>() : GetDataBlocks(releaseId, subject.Id);
            var dependentBlocks = new List<DependentDataBlock>();
            foreach (var block in blocks)
            {
                dependentBlocks.Add(await CreateDependentDataBlock(block));
            }

            return new DeleteDataBlockPlan()
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
                .AsQueryable()
                .Where(f => fileIds.Contains(f.Id))
                .ToListAsync();

            return new DependentDataBlock
            {
                Id = block.Id,
                Name = block.Name,
                ContentSectionHeading = GetContentSectionHeading(block),
                InfographicFilesInfo = files.Select(f => new InfographicFileInfo
                {
                    Id = f.Id,
                    Filename = f.Filename
                }).ToList(),
                IsKeyStatistic = await _context.KeyStatisticsDataBlock
                    .AnyAsync(ks => ks.DataBlockId == block.Id),
            };
        }

        private static string? GetContentSectionHeading(DataBlock block)
        {
            var section = block.ContentSection;

            if (section == null)
            {
                return null;
            }

            return section.Type switch
            {
                ContentSectionType.Generic => section.Heading,
                ContentSectionType.ReleaseSummary => "Release Summary",
                ContentSectionType.Headlines => "Headlines",
                ContentSectionType.KeyStatisticsSecondary => "Key Statistics",
                ContentSectionType.RelatedDashboards => "Related Dashboards",
                _ => section.Type.ToString()
            };
        }

        private async Task<Either<ActionResult, bool>> RemoveInfographicChartFromDataBlock(Guid releaseId, Guid id)
        {
            var blocks = GetDataBlocks(releaseId);

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
                    return true;
                }
            }

            return true;
        }

        private async Task RemoveChartFileReleaseLinks(DeleteDataBlockPlan deletePlan)
        {
            var chartFileIds = deletePlan.DependentDataBlocks.SelectMany(
                block => block.InfographicFilesInfo.Select(f => f.Id));

            await _releaseFileService.Delete(deletePlan.ReleaseId, chartFileIds);
        }

        private async Task<Either<ActionResult, Unit>> DeleteDependentDataBlocks(DeleteDataBlockPlan deletePlan)
        {
            var blockIdsToDelete = deletePlan
                .DependentDataBlocks
                .Select(block => block.Id);

            var dependentDataBlocks = _context
                .DataBlocks
                .AsQueryable()
                .Where(block => blockIdsToDelete.Contains(block.Id));

            _context.ContentBlocks.RemoveRange(dependentDataBlocks);
            await _context.SaveChangesAsync();
            return Unit.Instance;
        }

        private List<DataBlock> GetDataBlocks(Guid releaseId, Guid? subjectId = null)
        {
            return _context
                .ReleaseContentBlocks
                .Include(join => join.ContentBlock)
                .ThenInclude(block => block.ContentSection)
                .Where(join => join.ReleaseId == releaseId)
                .ToList()
                .Select(join => join.ContentBlock)
                .OfType<DataBlock>()
                .Where(block => subjectId == null || block.Query.SubjectId == subjectId)
                .ToList();
        }

        private async Task<Either<ActionResult, ReleaseContentBlock>> GetReleaseContentBlock(Guid dataBlockId)
        {
            return await _persistenceHelper.CheckEntityExists<ReleaseContentBlock>(
                query => query
                    .Include(rcb => rcb.Release)
                    .Include(rcb => rcb.ContentBlock)
                    .Where(rcb => rcb.ContentBlockId == dataBlockId && rcb.Release != null)
            );
        }

        private async Task<Either<ActionResult, DataBlock>> CheckIsDataBlock(ReleaseContentBlock releaseContentBlock)
        {
            if (releaseContentBlock.ContentBlock is DataBlock dataBlock)
            {
                return await Task.FromResult(dataBlock);
            }

            return new NotFoundResult();
        }

        private Task<Either<ActionResult, Unit>> InvalidateDataBlockCaches(DeleteDataBlockPlan deletePlan)
        {
            return deletePlan
                .DependentDataBlocks
                .ForEachAsync(dataBlock => InvalidateCachedDataBlock(deletePlan.ReleaseId, dataBlock.Id))
                .OnSuccessVoid();
        }

        public async Task InvalidateCachedDataBlocks(Guid releaseId)
        {
            var dataBlocks = GetDataBlocks(releaseId);
            foreach (var dataBlock in dataBlocks)
            {
                await InvalidateCachedDataBlock(releaseId, dataBlock.Id);
            }
        }

        private Task<Either<ActionResult, Unit>> InvalidateCachedDataBlock(Guid releaseId, Guid dataBlockId)
        {
            return _cacheKeyService
                .CreateCacheKeyForDataBlock(releaseId, dataBlockId)
                .OnSuccessVoid(_cacheService.DeleteItem);
        }

        public async Task<Either<ActionResult, List<DataBlockViewModel>>> GetUnattachedDataBlocks(Guid releaseId)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async release =>
                {
                    return await _context
                        .ReleaseContentBlocks
                        .Include(releaseContentBlock => releaseContentBlock.ContentBlock)
                        .Where(releaseContentBlock => releaseContentBlock.ReleaseId == release.Id)
                        .Select(releaseContentBlock => releaseContentBlock.ContentBlock)
                        .OfType<DataBlock>()
                        .ToAsyncEnumerable()
                        .WhereAwait(async dataBlock =>
                            await IsUnattachedDataBlock(releaseId, dataBlock))
                        .OrderBy(contentBlock => contentBlock.Name)
                        .Select(contentBlock => _mapper.Map<DataBlockViewModel>(contentBlock))
                        .ToListAsync();
                });
        }

        public async Task<bool> IsUnattachedDataBlock(Guid releaseId, DataBlock dataBlock)
        {
            return dataBlock.ContentSectionId == null
                   && await _context.KeyStatisticsDataBlock
                       .Where(ks =>ks.ReleaseId == releaseId)
                       .AllAsync(ks =>
                           ks.DataBlockId != dataBlock.Id);
        }
    }

    public class DependentDataBlock
    {
        [JsonIgnore] public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ContentSectionHeading { get; set; }
        public List<InfographicFileInfo> InfographicFilesInfo { get; set; } = new();
        public bool IsKeyStatistic { get; set; }
    }

    public class InfographicFileInfo
    {
        public Guid Id { get; set; }
        public string Filename { get; set; } = "";
    }

    public class DeleteDataBlockPlan
    {
        [JsonIgnore] public Guid ReleaseId { get; set; }
        public List<DependentDataBlock> DependentDataBlocks { get; set; } = new();
    }
}
