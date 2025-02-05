#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
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
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;
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
        private readonly IPrivateBlobCacheService _privateCacheService;
        private readonly ICacheKeyService _cacheKeyService;

        public DataBlockService(ContentDbContext context,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IReleaseFileService releaseFileService,
            IUserService userService,
            IMapper mapper,
            IPrivateBlobCacheService privateCacheService,
            ICacheKeyService cacheKeyService)
        {
            _context = context;
            _persistenceHelper = persistenceHelper;
            _releaseFileService = releaseFileService;
            _userService = userService;
            _mapper = mapper;
            _privateCacheService = privateCacheService;
            _cacheKeyService = cacheKeyService;
        }

        public async Task<Either<ActionResult, DataBlockViewModel>> Create(
            Guid releaseVersionId,
            DataBlockCreateRequest createRequest)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(async _ =>
                {
                    var dataBlock = _mapper.Map<DataBlock>(createRequest);
                    dataBlock.Id = Guid.NewGuid();
                    dataBlock.Created = DateTime.UtcNow;
                    dataBlock.ReleaseVersionId = releaseVersionId;

                    var dataBlockVersion = new DataBlockVersion
                    {
                        // EES-4640 - share the same Id with the underlying ContentBlock. This will make the process
                        // of removing DataBlock information out of the ContentBlocks table and into the
                        // DataBlockVersions table easier in future stages of extracting pure DataBlocks out of the
                        // Content model.
                        Id = dataBlock.Id,
                        ContentBlock = dataBlock,
                        ReleaseVersionId = releaseVersionId,
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

        public async Task<Either<ActionResult, Unit>> Delete(Guid releaseVersionId, Guid dataBlockVersionId)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccessDo(_userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(releaseVersion => GetDeletePlan(releaseVersionId: releaseVersion.Id,
                    dataBlockVersionId: dataBlockVersionId))
                .OnSuccessVoid(DeleteDataBlocks);
        }

        public Task<Either<ActionResult, Unit>> DeleteDataBlocks(DeleteDataBlockPlanViewModel deletePlan)
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

        public async Task<Either<ActionResult, Unit>> RemoveChartFile(Guid releaseVersionId, Guid fileId)
        {
            return await RemoveInfographicChartFromDataBlock(releaseVersionId: releaseVersionId, fileId: fileId)
                .OnSuccess(async () =>
                    await _releaseFileService.Delete(releaseVersionId: releaseVersionId, fileId: fileId));
        }

        public async Task<Either<ActionResult, DataBlockViewModel>> Get(Guid dataBlockVersionId)
        {
            return await GetDataBlockVersion(dataBlockVersionId)
                .OnSuccessDo(dataBlockVersion => _userService.CheckCanViewReleaseVersion(dataBlockVersion.ReleaseVersion))
                .OnSuccess(async dataBlockVersion =>
                {
                    var releaseVersionId = dataBlockVersion.ReleaseVersionId;

                    var viewModel = _mapper.Map<DataBlockViewModel>(dataBlockVersion);

                    var subjectId = dataBlockVersion.Query.SubjectId;

                    viewModel.DataSetId = subjectId;

                    viewModel.DataSetName = await _context.ReleaseFiles
                        .Where(rf =>
                            rf.ReleaseVersionId == releaseVersionId &&
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

        public async Task<Either<ActionResult, List<DataBlockSummaryViewModel>>> List(Guid releaseVersionId)
        {
            return await _persistenceHelper.CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(_userService.CheckCanViewReleaseVersion)
                .OnSuccess(async releaseVersion =>
                    {
                        var dataBlocks = await ListDataBlocks(releaseVersion.Id);

                        var dataBlockIdsAttachedToKeyStats = await _context.KeyStatisticsDataBlock
                            .Where(ks => ks.ReleaseVersionId == releaseVersion.Id)
                            .Select(ks => ks.DataBlockId)
                            .ToListAsync();

                        var featuredTables = await _context.FeaturedTables
                            .Where(ks => ks.ReleaseVersionId == releaseVersion.Id)
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
            Guid dataBlockVersionId,
            DataBlockUpdateRequest updateRequest)
        {
            return await GetDataBlockVersion(dataBlockVersionId)
                .OnSuccessDo(dataBlock => _userService.CheckCanUpdateReleaseVersion(dataBlock.ReleaseVersion))
                .OnSuccessDo(async dataBlockVersion =>
                {
                    // Remove old infographic file if using a new file
                    var infographicChart = dataBlockVersion.Charts.OfType<InfographicChart>().FirstOrDefault();
                    var updatedInfographicChart =
                        updateRequest.Charts.OfType<InfographicChart>().FirstOrDefault();

                    if (infographicChart != null &&
                        infographicChart.FileId != updatedInfographicChart?.FileId)
                    {
                        await _releaseFileService.Delete(releaseVersionId: dataBlockVersion.ReleaseVersionId,
                            fileId: new Guid(infographicChart.FileId));
                    }

                    // If has map chart, remove geojson cache
                    var mapChart = dataBlockVersion.Charts.OfType<MapChart>().FirstOrDefault();
                    if (mapChart != null)
                    {
                        await _privateCacheService.DeleteItemAsync(
                            new LocationsForDataBlockCacheKey(dataBlockVersion, mapChart.BoundaryLevel));
                    }

                    _mapper.Map(updateRequest, dataBlockVersion.ContentBlock);

                    _context.DataBlocks.Update(dataBlockVersion.ContentBlock);

                    await _context.SaveChangesAsync();
                })
                .OnSuccessDo(dataBlock => InvalidateCachedDataBlock(dataBlock.ReleaseVersionId, dataBlock.Id))
                .OnSuccess(() => Get(dataBlockVersionId));
        }

        public async Task<Either<ActionResult, DeleteDataBlockPlanViewModel>> GetDeletePlan(Guid releaseVersionId,
            Guid dataBlockVersionId)
        {
            return await _persistenceHelper
                .CheckEntityExists<DataBlockVersion>(
                    query => query
                        .Include(dataBlockVersion => dataBlockVersion.ReleaseVersion)
                        .Include(dataBlockVersion => dataBlockVersion.ContentBlock)
                        .ThenInclude(dataBlock => dataBlock.ContentSection)
                        .Where(dataBlockVersion => dataBlockVersion.ReleaseVersionId == releaseVersionId
                                                   && dataBlockVersion.Id == dataBlockVersionId)
                )
                .OnSuccessDo(dataBlockVersion => _userService.CheckCanUpdateReleaseVersion(dataBlockVersion.ReleaseVersion))
                .OnSuccess(async dataBlockVersion =>
                    new DeleteDataBlockPlanViewModel
                    {
                        ReleaseId = releaseVersionId,
                        DependentDataBlocks = new List<DependentDataBlock>
                        {
                            await CreateDependentDataBlock(dataBlockVersion)
                        }
                    }
                );
        }

        public Task<Either<ActionResult, DataBlockVersion>> GetDataBlockVersionForRelease(
            Guid releaseVersionId,
            Guid dataBlockParentId)
        {
            return _context
                .DataBlockVersions
                .Include(dataBlockVersion => dataBlockVersion.ReleaseVersion)
                .SingleOrDefaultAsync(dataBlockVersion => dataBlockVersion.ReleaseVersionId == releaseVersionId
                                                          && dataBlockVersion.DataBlockParentId == dataBlockParentId)
                .OrNotFound();
        }

        public async Task<DeleteDataBlockPlanViewModel> GetDeletePlan(Guid releaseVersionId, Subject? subject)
        {
            var dataBlockVersions = subject == null
                ? new List<DataBlockVersion>()
                : GetDataBlockVersions(releaseVersionId, subject.Id);
            var dependentBlocks = new List<DependentDataBlock>();
            foreach (var block in dataBlockVersions)
            {
                dependentBlocks.Add(await CreateDependentDataBlock(block));
            }

            return new DeleteDataBlockPlanViewModel
            {
                ReleaseId = releaseVersionId,
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

        private async Task<Either<ActionResult, bool>> RemoveInfographicChartFromDataBlock(Guid releaseVersionId,
            Guid fileId)
        {
            var dataBlockVersions = GetDataBlockVersions(releaseVersionId);

            foreach (var dataBlockVersion in dataBlockVersions)
            {
                // TODO EES-753 Alter this when multiple charts are supported
                var infoGraphicChart = dataBlockVersion.Charts
                    .OfType<InfographicChart>()
                    .FirstOrDefault();

                if (infoGraphicChart != null && infoGraphicChart.FileId == fileId.ToString())
                {
                    dataBlockVersion.Charts.Remove(infoGraphicChart);
                    _context.DataBlocks.Update(dataBlockVersion.ContentBlock);
                    await _context.SaveChangesAsync();
                    return true;
                }
            }

            return true;
        }

        private async Task RemoveChartFileReleaseLinks(DeleteDataBlockPlanViewModel deletePlan)
        {
            var chartFileIds = deletePlan.DependentDataBlocks.SelectMany(
                block => block.InfographicFilesInfo.Select(f => f.Id));

            await _releaseFileService.Delete(deletePlan.ReleaseId, chartFileIds);
        }

        private async Task<Either<ActionResult, Unit>> DeleteDependentDataBlocks(DeleteDataBlockPlanViewModel deletePlan)
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

        private List<DataBlockVersion> GetDataBlockVersions(Guid releaseVersionId, Guid? subjectId = null)
        {
            return _context
                .DataBlockVersions
                .Where(dataBlockVersion => dataBlockVersion.ReleaseVersionId == releaseVersionId)
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
                        .Include(dataBlockVersion => dataBlockVersion.ReleaseVersion)
                        .Where(dataBlockVersion => dataBlockVersion.Id == dataBlockId)
                );
        }

        private Task<Either<ActionResult, Unit>> InvalidateDataBlockCaches(DeleteDataBlockPlanViewModel deletePlan)
        {
            return deletePlan
                .DependentDataBlocks
                .ForEachAsync(dataBlock => InvalidateCachedDataBlock(deletePlan.ReleaseId, dataBlock.Id))
                .OnSuccessVoid();
        }

        public async Task InvalidateCachedDataBlocks(Guid releaseVersionId)
        {
            var dataBlocks = GetDataBlockVersions(releaseVersionId);
            foreach (var dataBlock in dataBlocks)
            {
                await InvalidateCachedDataBlock(releaseVersionId, dataBlock.Id);
            }
        }

        private Task<Either<ActionResult, Unit>> InvalidateCachedDataBlock(Guid releaseVersionId, Guid dataBlockId)
        {
            return _cacheKeyService
                .CreateCacheKeyForDataBlock(releaseVersionId: releaseVersionId,
                    dataBlockId: dataBlockId)
                .OnSuccessVoid(_privateCacheService.DeleteItemAsync);
        }

        public async Task<Either<ActionResult, List<DataBlockViewModel>>> GetUnattachedDataBlocks(Guid releaseVersionId)
        {
            return await _persistenceHelper.CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(_userService.CheckCanViewReleaseVersion)
                .OnSuccess(async releaseVersion =>
                {
                    return await _context
                        .DataBlockVersions
                        .Where(block => block.ReleaseVersionId == releaseVersion.Id)
                        .ToAsyncEnumerable()
                        .WhereAwait(async dataBlockVersion =>
                            await IsUnattachedDataBlock(releaseVersionId, dataBlockVersion))
                        .OrderBy(dataBlockVersion => dataBlockVersion.Name)
                        .Select(dataBlockVersion => _mapper.Map<DataBlockViewModel>(dataBlockVersion))
                        .ToListAsync();
                });
        }

        public async Task<bool> IsUnattachedDataBlock(Guid releaseVersionId, DataBlockVersion dataBlockVersion)
        {
            return dataBlockVersion.ContentSectionId == null
                   && await _context.KeyStatisticsDataBlock
                       .Where(ks => ks.ReleaseVersionId == releaseVersionId)
                       .AllAsync(ks =>
                           ks.DataBlockId != dataBlockVersion.Id);
        }

        public async Task<List<DataBlock>> ListDataBlocks(Guid releaseVersionId)
        {
            return await _context
                .ContentBlocks
                .Where(block => block.ReleaseVersionId == releaseVersionId)
                .OfType<DataBlock>()
                .ToListAsync();
        }
    }
}
