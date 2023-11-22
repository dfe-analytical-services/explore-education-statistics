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
        private readonly IBlobCacheService _cacheService;
        private readonly ICacheKeyService _cacheKeyService;

        public DataBlockService(ContentDbContext context,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IReleaseFileService releaseFileService,
            IUserService userService,
            IMapper mapper,
            IBlobCacheService cacheService,
            ICacheKeyService cacheKeyService)
        {
            _context = context;
            _persistenceHelper = persistenceHelper;
            _releaseFileService = releaseFileService;
            _userService = userService;
            _mapper = mapper;
            _cacheService = cacheService;
            _cacheKeyService = cacheKeyService;
        }

        public async Task<Either<ActionResult, DataBlockViewModel>> Create(
            Guid releaseId,
            DataBlockCreateViewModel dataBlockCreate)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async _ =>
                {
                    var dataBlock = _mapper.Map<DataBlock>(dataBlockCreate);
                    dataBlock.Id = Guid.NewGuid();
                    dataBlock.Created = DateTime.UtcNow;
                    dataBlock.ReleaseId = releaseId;

                    var dataBlockVersion = new DataBlockVersion
                    {
                        // EES-4640 - share the same Id with the underlying ContentBlock. This will make the process
                        // of removing DataBlock information out of the ContentBlocks table and into the
                        // DataBlockVersions table easier in future stages of extracting pure DataBlocks out of the
                        // Content model.
                        Id = dataBlock.Id,
                        ContentBlock = dataBlock,
                        ReleaseId = releaseId,
                        Created = DateTime.UtcNow,
                        DataBlockParent = new()
                    };

                    await _context.DataBlockVersions.AddAsync(dataBlockVersion);
                    await _context.SaveChangesAsync();

                    dataBlockVersion.DataBlockParent.LatestDraftVersion = dataBlockVersion;
                    _context.DataBlockParents.Update(dataBlockVersion.DataBlockParent);
                    await _context.SaveChangesAsync();

                    return await Get(dataBlock.Id);
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

                    var featuredTables = _context.FeaturedTables
                        .Where(ft => dataBlockIds.Contains(ft.DataBlockId));
                    _context.FeaturedTables.RemoveRange(featuredTables);

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

        public async Task<Either<ActionResult, DataBlockViewModel>> Get(Guid dataBlockVersionId)
        {
            return await GetDataBlockVersion(dataBlockVersionId)
                .OnSuccessDo(dataBlockVersion => _userService.CheckCanViewRelease(dataBlockVersion.Release))
                .OnSuccess(async dataBlockVersion =>
                {
                    var releaseId = dataBlockVersion.ReleaseId;

                    var viewModel = _mapper.Map<DataBlockViewModel>(dataBlockVersion);

                    var subjectId = dataBlockVersion.Query.SubjectId;

                    viewModel.DataSetId = subjectId;

                    viewModel.DataSetName = await _context.ReleaseFiles
                        .Where(rf =>
                            rf.ReleaseId == releaseId &&
                            rf.File.SubjectId == subjectId &&
                            rf.File.Type == FileType.Data)
                        .Select(rf => rf.Name)
                        .SingleAsync() ?? "";

                    var featuredTable = await _context.FeaturedTables.SingleOrDefaultAsync(
                        ft => ft.DataBlockId == dataBlockVersion.Id);

                    if (featuredTable != null)
                    {
                        viewModel.HighlightName = featuredTable.Name;
                        viewModel.HighlightDescription = featuredTable.Description;
                    }

                    return viewModel;
                });
        }

        public async Task<Either<ActionResult, List<DataBlockSummaryViewModel>>> List(Guid releaseId)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async release =>
                    {
                        var dataBlocks = await ListDataBlocks(release.Id);

                        var dataBlockIdsAttachedToKeyStats = await _context.KeyStatisticsDataBlock
                            .Where(ks => ks.ReleaseId == release.Id)
                            .Select(ks => ks.DataBlockId)
                            .ToListAsync();

                        var featuredTables = await _context.FeaturedTables
                            .Where(ks => ks.ReleaseId == release.Id)
                            .ToListAsync();

                        return dataBlocks.Select(block =>
                            {
                                var featuredTable = featuredTables
                                    .SingleOrDefault(ft => ft.DataBlockId == block.Id);

                                var inContent = block.ContentSectionId != null
                                                || dataBlockIdsAttachedToKeyStats.Contains(block.Id);
                                return new DataBlockSummaryViewModel
                                {
                                    Id = block.Id,
                                    Heading = block.Heading,
                                    Name = block.Name,
                                    Created = block.Created,
                                    HighlightName = featuredTable?.Name ?? null,
                                    HighlightDescription = featuredTable?.Description ?? null,
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
            return await GetDataBlockVersion(id)
                .OnSuccessDo(dataBlock => _userService.CheckCanUpdateRelease(dataBlock.Release))
                .OnSuccessDo(async dataBlockVersion =>
                {
                    // TODO EES-753 Alter this when multiple charts are supported
                    var infographicChart = dataBlockVersion.Charts.OfType<InfographicChart>().FirstOrDefault();
                    var updatedInfographicChart =
                        dataBlockUpdate.Charts.OfType<InfographicChart>().FirstOrDefault();

                    if (infographicChart != null &&
                        infographicChart.FileId != updatedInfographicChart?.FileId)
                    {
                        await _releaseFileService.Delete(dataBlockVersion.ReleaseId, new Guid(infographicChart.FileId));
                    }

                    _mapper.Map(dataBlockUpdate, dataBlockVersion.ContentBlock);

                    _context.DataBlocks.Update(dataBlockVersion.ContentBlock);

                    await _context.SaveChangesAsync();
                })
                .OnSuccessDo(dataBlock => InvalidateCachedDataBlock(dataBlock.ReleaseId, dataBlock.Id))
                .OnSuccess(() => Get(id));
        }

        public async Task<Either<ActionResult, DeleteDataBlockPlan>> GetDeletePlan(Guid releaseId, Guid dataBlockId)
        {
            return await _persistenceHelper
                .CheckEntityExists<DataBlockVersion>(
                    query => query
                        .Include(dataBlockVersion => dataBlockVersion.Release)
                        .Include(dataBlockVersion => dataBlockVersion.ContentBlock)
                        .ThenInclude(dataBlock => dataBlock.ContentSection)
                        .Where(dataBlockVersion => dataBlockVersion.ReleaseId == releaseId
                                                   && dataBlockVersion.Id == dataBlockId)
                )
                .OnSuccessDo(dataBlockVersion => _userService.CheckCanUpdateRelease(dataBlockVersion.Release))
                .OnSuccess(async dataBlockVersion =>
                    new DeleteDataBlockPlan
                    {
                        ReleaseId = releaseId,
                        DependentDataBlocks = new List<DependentDataBlock>
                        {
                            await CreateDependentDataBlock(dataBlockVersion)
                        }
                    }
                );
        }

        public Task<Either<ActionResult, DataBlockVersion>> GetDataBlockVersionForRelease(
            Guid releaseId,
            Guid dataBlockParentId)
        {
            return _context
                .DataBlockVersions
                .Include(dataBlockVersion => dataBlockVersion.Release)
                .SingleOrDefaultAsync(dataBlockVersion => dataBlockVersion.ReleaseId == releaseId
                                                          && dataBlockVersion.DataBlockParentId == dataBlockParentId)
                .OrNotFound();
        }

        public async Task<DeleteDataBlockPlan> GetDeletePlan(Guid releaseId, Subject? subject)
        {
            var dataBlockVersions = subject == null ? new List<DataBlockVersion>() : GetDataBlockVersions(releaseId, subject.Id);
            var dependentBlocks = new List<DependentDataBlock>();
            foreach (var block in dataBlockVersions)
            {
                dependentBlocks.Add(await CreateDependentDataBlock(block));
            }

            return new DeleteDataBlockPlan()
            {
                ReleaseId = releaseId,
                DependentDataBlocks = dependentBlocks
            };
        }

        private async Task<DependentDataBlock> CreateDependentDataBlock(DataBlockVersion dataBlockVersion)
        {
            var fileIds = dataBlockVersion
                .Charts
                .OfType<InfographicChart>()
                .Select(chart => new Guid(chart.FileId))
                .ToList();

            var files = await _context.Files
                .AsQueryable()
                .Where(f => fileIds.Contains(f.Id))
                .ToListAsync();

            var featuredTable = await _context.FeaturedTables
                .SingleOrDefaultAsync(ft => ft.DataBlockId == dataBlockVersion.Id);

            return new DependentDataBlock
            {
                Id = dataBlockVersion.Id,
                Name = dataBlockVersion.Name,
                ContentSectionHeading = GetContentSectionHeading(dataBlockVersion),
                InfographicFilesInfo = files.Select(f => new InfographicFileInfo
                {
                    Id = f.Id,
                    Filename = f.Filename
                }).ToList(),
                IsKeyStatistic = await _context.KeyStatisticsDataBlock
                    .AnyAsync(ks => ks.DataBlockId == dataBlockVersion.Id),
                FeaturedTable = featuredTable != null
                    ? new FeaturedTableBasicViewModel(
                        Name: featuredTable.Name,
                        Description: featuredTable.Description)
                    : null,
            };
        }

        private static string? GetContentSectionHeading(DataBlockVersion dataBlockVersion)
        {
            var section = dataBlockVersion.ContentSection;

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
            var dataBlockVersions = GetDataBlockVersions(releaseId);

            foreach (var dataBlockVersion in dataBlockVersions)
            {
                // TODO EES-753 Alter this when multiple charts are supported
                var infoGraphicChart = dataBlockVersion.Charts
                    .OfType<InfographicChart>()
                    .FirstOrDefault();

                if (infoGraphicChart != null && infoGraphicChart.FileId == id.ToString())
                {
                    dataBlockVersion.Charts.Remove(infoGraphicChart);
                    _context.DataBlocks.Update(dataBlockVersion.ContentBlock);
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

            var dependentDataBlockVersions = await _context
                .DataBlockVersions
                .Include(dataBlockVersion => dataBlockVersion.DataBlockParent)
                .Where(dataBlockVersion => blockIdsToDelete.Contains(dataBlockVersion.Id))
                .ToListAsync();

            var dataBlockParents = dependentDataBlockVersions
                .Select(dataBlockVersion => dataBlockVersion.DataBlockParent)
                .ToList();

            // Set all of the DataBlockParents' "LatestDraftVersion" versions to null, to indicate that these Data
            // Blocks are no longer a part of this Release (or amendment).
            dataBlockParents.ForEach(dataBlockParent => dataBlockParent.LatestDraftVersionId = null);

            await _context.SaveChangesAsync();

            // Delete the DataBlockVersion and its associated ContentBlock of type "DataBlock".
            _context.DataBlockVersions.RemoveRange(dependentDataBlockVersions);
            _context.ContentBlocks.RemoveRange(dependentDataBlockVersions
                .Select(dataBlockVersion => dataBlockVersion.ContentBlock));

            // If the DataBlockVersion that has just been deleted is the only version under its DataBlockParent (i.e.
            // it's the LatestVersion but there isn't another already-published version), also delete its parent.
            var orphanedDataBlockParents = dataBlockParents
                .Where(dataBlockParent => dataBlockParent.LatestPublishedVersionId == null)
                .ToList();

            _context.DataBlockParents.RemoveRange(orphanedDataBlockParents);

            await _context.SaveChangesAsync();
            return Unit.Instance;
        }

        private List<DataBlockVersion> GetDataBlockVersions(Guid releaseId, Guid? subjectId = null)
        {
            return _context
                .DataBlockVersions
                .Where(dataBlockVersion => dataBlockVersion.ReleaseId == releaseId)
                // Pull these results into memory so that the Query field (which is JSON) can be queried.
                .ToList()
                .Where(dataBlockVersion => subjectId == null || dataBlockVersion.Query.SubjectId == subjectId)
                .ToList();
        }

        private async Task<Either<ActionResult, DataBlockVersion>> GetDataBlockVersion(Guid dataBlockId)
        {
            return await _persistenceHelper
                .CheckEntityExists<DataBlockVersion>(
                    query => query
                        .Include(dataBlockVersion => dataBlockVersion.Release)
                        .Where(dataBlockVersion => dataBlockVersion.Id == dataBlockId)
                );
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
            var dataBlocks = GetDataBlockVersions(releaseId);
            foreach (var dataBlock in dataBlocks)
            {
                await InvalidateCachedDataBlock(releaseId, dataBlock.Id);
            }
        }

        private Task<Either<ActionResult, Unit>> InvalidateCachedDataBlock(Guid releaseId, Guid dataBlockId)
        {
            return _cacheKeyService
                .CreateCacheKeyForDataBlock(releaseId, dataBlockId)
                .OnSuccessVoid(_cacheService.DeleteItemAsync);
        }

        public async Task<Either<ActionResult, List<DataBlockViewModel>>> GetUnattachedDataBlocks(Guid releaseId)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async release =>
                {
                    return await _context
                        .DataBlockVersions
                        .Where(block => block.ReleaseId == release.Id)
                        .ToAsyncEnumerable()
                        .WhereAwait(async dataBlockVersion => await IsUnattachedDataBlock(releaseId, dataBlockVersion))
                        .OrderBy(dataBlockVersion => dataBlockVersion.Name)
                        .Select(dataBlockVersion => _mapper.Map<DataBlockViewModel>(dataBlockVersion))
                        .ToListAsync();
                });
        }

        public async Task<bool> IsUnattachedDataBlock(Guid releaseId, DataBlockVersion dataBlockVersion)
        {
            return dataBlockVersion.ContentSectionId == null
                   && await _context.KeyStatisticsDataBlock
                       .Where(ks =>ks.ReleaseId == releaseId)
                       .AllAsync(ks =>
                           ks.DataBlockId != dataBlockVersion.Id);
        }

        public async Task<List<DataBlock>> ListDataBlocks(Guid releaseId)
        {
            return await _context
                    .ContentBlocks
                    .Where(block => block.ReleaseId == releaseId)
                    .OfType<DataBlock>()
                    .ToListAsync();
        }
    }

    public class DependentDataBlock
    {
        [JsonIgnore] public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ContentSectionHeading { get; set; }
        public List<InfographicFileInfo> InfographicFilesInfo { get; set; } = new();
        public bool IsKeyStatistic { get; set; }
        public FeaturedTableBasicViewModel? FeaturedTable { get; set; }
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
