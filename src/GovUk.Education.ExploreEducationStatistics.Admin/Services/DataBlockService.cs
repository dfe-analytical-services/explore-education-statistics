#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
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
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataBlockService : IDataBlockService
{
    private readonly ContentDbContext _context;
    private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
    private readonly IReleaseFileService _releaseFileService;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IPrivateBlobCacheService _privateCacheService;
    private readonly ICacheKeyService _cacheKeyService;

    public DataBlockService(
        ContentDbContext context,
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IReleaseFileService releaseFileService,
        IUserService userService,
        IMapper mapper,
        IPrivateBlobCacheService privateCacheService,
        ICacheKeyService cacheKeyService
    )
    {
        _context = context;
        _persistenceHelper = persistenceHelper;
        _releaseFileService = releaseFileService;
        _userService = userService;
        _mapper = mapper;
        _privateCacheService = privateCacheService;
        _cacheKeyService = cacheKeyService;
    }

    public async Task<Either<ActionResult, DataBlockVersionViewModel>> Create(
        Guid releaseVersionId,
        DataBlockCreateRequest createRequest
    )
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, query => query.Include(rv => rv.Release))
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async _ =>
            {
                var dataBlockVersion = _mapper.Map<DataBlockVersion>(createRequest);
                dataBlockVersion.Id = Guid.NewGuid();
                dataBlockVersion.Created = DateTime.UtcNow;
                dataBlockVersion.ReleaseVersionId = releaseVersionId;

                // DataBlock and DataBlockVersion reference each other, so the two rows cannot be inserted
                // in a single SaveChanges - EF rejects the graph as a circular dependency. Insert the parent
                // without its LatestDraftVersion first, then point it at the newly inserted version.
                dataBlockVersion.DataBlock = new DataBlock { LatestDraftVersionId = null };

                await _context.RequireTransaction(async () =>
                {
                    await _context.AddAsync(dataBlockVersion);
                    await _context.SaveChangesAsync();

                    dataBlockVersion.DataBlock.LatestDraftVersion = dataBlockVersion;
                    dataBlockVersion.DataBlock.LatestDraftVersionId = dataBlockVersion.Id;
                    await _context.SaveChangesAsync();
                });

                return await Get(dataBlockVersion.Id);
            });
    }

    public async Task<Either<ActionResult, Unit>> Delete(Guid releaseVersionId, Guid dataBlockVersionId)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, query => query.Include(rv => rv.Release))
            .OnSuccessDo(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(releaseVersion =>
                GetDeletePlan(releaseVersionId: releaseVersion.Id, dataBlockVersionId: dataBlockVersionId)
            )
            .OnSuccessVoid(DeleteDataBlockVersions);
    }

    public Task<Either<ActionResult, Unit>> DeleteDataBlockVersions(DeleteDataBlockPlanViewModel deletePlan)
    {
        return InvalidateDataBlockCaches(deletePlan)
            .OnSuccessVoid(async () =>
            {
                var dataBlockVersionIds = deletePlan.DependentDataBlocks.Select(db => db.Id).ToList();

                var keyStats = _context.KeyStatisticsDataBlock.Where(ks =>
                    dataBlockVersionIds.Contains(ks.DataBlockVersionId)
                );
                _context.KeyStatisticsDataBlock.RemoveRange(keyStats);

                var featuredTables = _context.FeaturedTables.Where(ft =>
                    dataBlockVersionIds.Contains(ft.DataBlockVersionId)
                );
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
                await _releaseFileService.Delete(releaseVersionId: releaseVersionId, fileId: fileId)
            );
    }

    public async Task<Either<ActionResult, DataBlockVersionViewModel>> Get(Guid dataBlockVersionId)
    {
        return await GetDataBlockVersion(dataBlockVersionId)
            .OnSuccessDo(dataBlockVersion => _userService.CheckCanViewReleaseVersion(dataBlockVersion.ReleaseVersion))
            .OnSuccess(async dataBlockVersion =>
            {
                var releaseVersionId = dataBlockVersion.ReleaseVersionId;

                // Only a data block that is placed in a content section has a DataBlockVersionLink. An unattached
                // one has no positional or locking state, so it is mapped straight from its DataBlockVersion.
                var link = await _context.DataBlockVersionLinks.SingleOrDefaultAsync(link =>
                    link.DataBlockVersionId == dataBlockVersionId
                );

                var viewModel = link is not null
                    ? _mapper.Map<DataBlockVersionViewModel>(link)
                    : _mapper.Map<DataBlockVersionViewModel>(dataBlockVersion);

                var subjectId = dataBlockVersion.Query.SubjectId;

                viewModel.DataSetId = subjectId;

                viewModel.DataSetName =
                    await _context
                        .ReleaseFiles.Where(rf =>
                            rf.ReleaseVersionId == releaseVersionId
                            && rf.File.SubjectId == subjectId
                            && rf.File.Type == FileType.Data
                        )
                        .Select(rf => rf.Name)
                        .SingleAsync()
                    ?? "";

                var featuredTable = await _context.FeaturedTables.SingleOrDefaultAsync(ft =>
                    ft.DataBlockVersionId == dataBlockVersionId
                );

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
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, query => query.Include(rv => rv.Release))
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                var dataBlocks = await ListDataBlocks(releaseVersion.Id);

                var dataBlockVersionIdsAttachedToKeyStats = await _context
                    .KeyStatisticsDataBlock.Where(ks => ks.ReleaseVersionId == releaseVersion.Id)
                    .Select(ks => ks.DataBlockVersionId)
                    .ToListAsync();

                var featuredTables = await _context
                    .FeaturedTables.Where(ks => ks.ReleaseVersionId == releaseVersion.Id)
                    .ToListAsync();

                // A DataBlockVersionLink exists only for as long as its DataBlockVersion is placed in a content
                // section, so the presence of a link is what makes a data block "in content".
                var dataBlockVersionIdsInContent = await _context
                    .DataBlockVersionLinks.Where(link => link.ReleaseVersionId == releaseVersion.Id)
                    .Select(link => link.DataBlockVersionId)
                    .ToListAsync();

                return dataBlocks
                    .Select(block =>
                    {
                        var featuredTable = featuredTables.SingleOrDefault(ft => ft.DataBlockVersionId == block.Id);

                        var inContent =
                            dataBlockVersionIdsInContent.Contains(block.Id)
                            || dataBlockVersionIdsAttachedToKeyStats.Contains(block.Id);
                        return new DataBlockSummaryViewModel
                        {
                            Id = block.Id,
                            Heading = block.Heading,
                            Name = block.Name,
                            Created = block.Created,
                            HighlightName = featuredTable?.Name,
                            HighlightDescription = featuredTable?.Description,
                            Source = block.Source,
                            ChartsCount = block.Charts.Count,
                            InContent = inContent,
                        };
                    })
                    .OrderBy(model => model.Name)
                    .ToList();
            });
    }

    public async Task<Either<ActionResult, DataBlockVersionViewModel>> Update(
        Guid dataBlockVersionId,
        DataBlockUpdateRequest updateRequest
    )
    {
        return await GetDataBlockVersion(dataBlockVersionId)
            .OnSuccessDo(dataBlockVersion => _userService.CheckCanUpdateReleaseVersion(dataBlockVersion.ReleaseVersion))
            .OnSuccessDo(async dataBlockVersion =>
            {
                // Remove old infographic file if using a new file
                var infographicChart = dataBlockVersion.Charts.OfType<InfographicChart>().FirstOrDefault();
                var updatedInfographicChart = updateRequest.Charts.OfType<InfographicChart>().FirstOrDefault();

                if (infographicChart != null && infographicChart.FileId != updatedInfographicChart?.FileId)
                {
                    await _releaseFileService.Delete(
                        releaseVersionId: dataBlockVersion.ReleaseVersionId,
                        fileId: new Guid(infographicChart.FileId)
                    );
                }

                // If has map chart, remove geojson cache
                var mapChart = dataBlockVersion.Charts.OfType<MapChart>().FirstOrDefault();
                if (mapChart != null)
                {
                    await _privateCacheService.DeleteItemAsync(
                        new LocationsForDataBlockCacheKey(dataBlockVersion, mapChart.BoundaryLevel)
                    );
                }

                _mapper.Map(updateRequest, dataBlockVersion);

                _context.DataBlockVersions.Update(dataBlockVersion);

                await _context.SaveChangesAsync();
            })
            .OnSuccessDo(dataBlockVersion =>
                InvalidateCachedDataBlock(dataBlockVersion.ReleaseVersionId, dataBlockVersion.Id)
            )
            .OnSuccess(() => Get(dataBlockVersionId));
    }

    public async Task<Either<ActionResult, DeleteDataBlockPlanViewModel>> GetDeletePlan(
        Guid releaseVersionId,
        Guid dataBlockVersionId
    )
    {
        return await _persistenceHelper
            .CheckEntityExists<DataBlockVersion>(query =>
                query
                    .Include(dataBlockVersion => dataBlockVersion.ReleaseVersion)
                        .ThenInclude(releaseVersion => releaseVersion.Release)
                    .Where(dataBlockVersion =>
                        dataBlockVersion.ReleaseVersionId == releaseVersionId
                        && dataBlockVersion.Id == dataBlockVersionId
                    )
            )
            .OnSuccessDo(dataBlockVersion => _userService.CheckCanUpdateReleaseVersion(dataBlockVersion.ReleaseVersion))
            .OnSuccess(async dataBlockVersion => new DeleteDataBlockPlanViewModel
            {
                ReleaseId = releaseVersionId,
                DependentDataBlocks = new List<DependentDataBlock> { await CreateDependentDataBlock(dataBlockVersion) },
            });
    }

    public Task<Either<ActionResult, DataBlockVersion>> GetDataBlockVersionForRelease(
        Guid releaseVersionId,
        Guid dataBlockId
    )
    {
        return _context
            .DataBlockVersions.Include(dataBlockVersion => dataBlockVersion.ReleaseVersion)
                .ThenInclude(rv => rv.Release)
            .SingleOrDefaultAsync(dataBlockVersion =>
                dataBlockVersion.ReleaseVersionId == releaseVersionId && dataBlockVersion.DataBlockId == dataBlockId
            )
            .OrNotFound();
    }

    public async Task<DeleteDataBlockPlanViewModel> GetDeletePlan(Guid releaseVersionId, Subject? subject)
    {
        var dataBlockVersions =
            subject == null ? new List<DataBlockVersion>() : GetDataBlockVersions(releaseVersionId, subject.Id);
        var dependentBlocks = new List<DependentDataBlock>();
        foreach (var block in dataBlockVersions)
        {
            dependentBlocks.Add(await CreateDependentDataBlock(block));
        }

        return new DeleteDataBlockPlanViewModel { ReleaseId = releaseVersionId, DependentDataBlocks = dependentBlocks };
    }

    private async Task<DependentDataBlock> CreateDependentDataBlock(DataBlockVersion dataBlockVersion)
    {
        var fileIds = dataBlockVersion
            .Charts.OfType<InfographicChart>()
            .Select(chart => new Guid(chart.FileId))
            .ToList();

        var files = await _context.Files.AsQueryable().Where(f => fileIds.Contains(f.Id)).ToListAsync();

        var featuredTable = await _context.FeaturedTables.SingleOrDefaultAsync(ft =>
            ft.DataBlockVersionId == dataBlockVersion.Id
        );

        var contentSection = await GetContentSectionForDataBlockVersion(dataBlockVersion.Id);

        return new DependentDataBlock
        {
            Id = dataBlockVersion.Id,
            Name = dataBlockVersion.Name,
            ContentSectionHeading = GetContentSectionHeading(contentSection),
            InfographicFilesInfo = files
                .Select(f => new InfographicFileInfo { Id = f.Id, Filename = f.Filename })
                .ToList(),
            IsKeyStatistic = await _context.KeyStatisticsDataBlock.AnyAsync(ks =>
                ks.DataBlockVersionId == dataBlockVersion.Id
            ),
            FeaturedTable =
                featuredTable != null
                    ? new FeaturedTableBasicViewModel(Name: featuredTable.Name, Description: featuredTable.Description)
                    : null,
        };
    }

    private Task<ContentSection?> GetContentSectionForDataBlockVersion(Guid dataBlockVersionId)
    {
        return _context
            .DataBlockVersionLinks.Where(link => link.DataBlockVersionId == dataBlockVersionId)
            .Select(link => link.ContentSection)
            .SingleOrDefaultAsync();
    }

    private static string? GetContentSectionHeading(ContentSection? section)
    {
        return section?.Type switch
        {
            null => null,
            ContentSectionType.Generic => section.Heading,
            ContentSectionType.Headlines => "Headlines",
            ContentSectionType.KeyStatisticsSecondary => "Key Statistics",
            // The other types of section don't support adding DataBlocks, so don't expect to encounter them here
            _ => throw new InvalidOperationException(
                $"Unexpected ContentSectionType {section.Type} for ContentSection with id {section.Id}"
            ),
        };
    }

    private async Task<Either<ActionResult, bool>> RemoveInfographicChartFromDataBlock(
        Guid releaseVersionId,
        Guid fileId
    )
    {
        var dataBlockVersions = GetDataBlockVersions(releaseVersionId);

        foreach (var dataBlockVersion in dataBlockVersions)
        {
            // TODO EES-753 Alter this when multiple charts are supported
            var infoGraphicChart = dataBlockVersion.Charts.OfType<InfographicChart>().FirstOrDefault();

            if (infoGraphicChart != null && infoGraphicChart.FileId == fileId.ToString())
            {
                dataBlockVersion.Charts.Remove(infoGraphicChart);
                _context.DataBlockVersions.Update(dataBlockVersion);
                await _context.SaveChangesAsync();
                return true;
            }
        }

        return true;
    }

    private async Task RemoveChartFileReleaseLinks(DeleteDataBlockPlanViewModel deletePlan)
    {
        var chartFileIds = deletePlan.DependentDataBlocks.SelectMany(block =>
            block.InfographicFilesInfo.Select(f => f.Id)
        );

        await _releaseFileService.Delete(deletePlan.ReleaseId, chartFileIds);
    }

    private async Task<Either<ActionResult, Unit>> DeleteDependentDataBlocks(DeleteDataBlockPlanViewModel deletePlan)
    {
        var blockIdsToDelete = deletePlan.DependentDataBlocks.Select(block => block.Id);

        var dependentDataBlockVersions = await _context
            .DataBlockVersions.Include(dataBlockVersion => dataBlockVersion.DataBlock)
            .Where(dataBlockVersion => blockIdsToDelete.Contains(dataBlockVersion.Id))
            .ToListAsync();

        var dataBlocks = dependentDataBlockVersions.Select(dataBlockVersion => dataBlockVersion.DataBlock).ToList();

        // Set all of the DataBlocks' "LatestDraftVersion" versions to null, to indicate that these Data
        // Blocks are no longer a part of this Release (or amendment).
        dataBlocks.ForEach(dataBlock => dataBlock.LatestDraftVersionId = null);

        await _context.SaveChangesAsync();

        // Delete the DataBlockVersion and its associated DataBlockVersionLink (a ContentBlock). As the link -> version
        // relationship is NoAction (see ContentDbContext.ConfigureDataBlockVersionLink), the link must be removed
        // explicitly; EF orders the deletes so the dependent link is removed before its principal version.
        var dependentDataBlockVersionIds = dependentDataBlockVersions
            .Select(dataBlockVersion => dataBlockVersion.Id)
            .ToList();
        var dependentDataBlockVersionLinks = await _context
            .DataBlockVersionLinks.Where(link => dependentDataBlockVersionIds.Contains(link.DataBlockVersionId))
            .ToListAsync();
        _context.ContentBlocks.RemoveRange(dependentDataBlockVersionLinks);
        _context.DataBlockVersions.RemoveRange(dependentDataBlockVersions);

        // If the DataBlockVersion that has just been deleted is the only version under its DataBlock (i.e.
        // it's the LatestVersion but there isn't another already-published version), also delete its parent.
        var orphanedDataBlocks = dataBlocks.Where(dataBlock => dataBlock.LatestPublishedVersionId == null).ToList();

        _context.DataBlocks.RemoveRange(orphanedDataBlocks);

        await _context.SaveChangesAsync();
        return Unit.Instance;
    }

    private List<DataBlockVersion> GetDataBlockVersions(Guid releaseVersionId, Guid? subjectId = null)
    {
        return _context
            .DataBlockVersions.Where(dataBlockVersion => dataBlockVersion.ReleaseVersionId == releaseVersionId)
            // Pull these results into memory so that the Query field (which is JSON) can be queried.
            .ToList()
            .Where(dataBlockVersion => subjectId == null || dataBlockVersion.Query.SubjectId == subjectId)
            .ToList();
    }

    private async Task<Either<ActionResult, DataBlockVersion>> GetDataBlockVersion(Guid dataBlockVersionId)
    {
        return await _persistenceHelper.CheckEntityExists<DataBlockVersion>(query =>
            query
                .Include(dataBlockVersion => dataBlockVersion.ReleaseVersion)
                    .ThenInclude(rv => rv.Release)
                .Where(dataBlockVersion => dataBlockVersion.Id == dataBlockVersionId)
        );
    }

    private Task<Either<ActionResult, Unit>> InvalidateDataBlockCaches(DeleteDataBlockPlanViewModel deletePlan)
    {
        return deletePlan
            .DependentDataBlocks.ForEachAsync(dataBlock =>
                InvalidateCachedDataBlock(deletePlan.ReleaseId, dataBlock.Id)
            )
            .OnSuccessVoid();
    }

    public async Task InvalidateCachedDataBlocks(Guid releaseVersionId)
    {
        var dataBlockVersions = GetDataBlockVersions(releaseVersionId);
        foreach (var dataBlockVersion in dataBlockVersions)
        {
            await InvalidateCachedDataBlock(releaseVersionId, dataBlockVersion.Id);
        }
    }

    private Task<Either<ActionResult, Unit>> InvalidateCachedDataBlock(Guid releaseVersionId, Guid dataBlockVersionId)
    {
        return _cacheKeyService
            .CreateCacheKeyForDataBlock(releaseVersionId: releaseVersionId, dataBlockVersionId: dataBlockVersionId)
            .OnSuccessVoid(_privateCacheService.DeleteItemAsync);
    }

    public async Task<Either<ActionResult, List<DataBlockVersionViewModel>>> GetUnattachedDataBlocks(
        Guid releaseVersionId
    )
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, query => query.Include(rv => rv.Release))
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                var dataBlockVersionIdsAttachedToKeyStats = await _context
                    .KeyStatisticsDataBlock.Where(ks => ks.ReleaseVersionId == releaseVersion.Id)
                    .Select(ks => ks.DataBlockVersionId)
                    .ToListAsync();

                // A DataBlockVersion that is placed in a content section has a DataBlockVersionLink, so the versions
                // without one are the unattached data blocks.
                var dataBlockVersionIdsInContent = await _context
                    .DataBlockVersionLinks.Where(link => link.ReleaseVersionId == releaseVersion.Id)
                    .Select(link => link.DataBlockVersionId)
                    .ToListAsync();

                var attachedDataBlockVersionIds = dataBlockVersionIdsAttachedToKeyStats
                    .Concat(dataBlockVersionIdsInContent)
                    .ToList();

                var unattachedDataBlockVersions = await _context
                    .DataBlockVersions.Where(dataBlockVersion =>
                        dataBlockVersion.ReleaseVersionId == releaseVersion.Id
                        && !attachedDataBlockVersionIds.Contains(dataBlockVersion.Id)
                    )
                    .ToListAsync();

                return unattachedDataBlockVersions
                    .OrderBy(dataBlockVersion => dataBlockVersion.Name)
                    .Select(dataBlockVersion => _mapper.Map<DataBlockVersionViewModel>(dataBlockVersion))
                    .ToList();
            });
    }

    public async Task<bool> IsUnattachedDataBlock(Guid releaseVersionId, DataBlockVersion dataBlockVersion)
    {
        // A DataBlockVersion only has a DataBlockVersionLink for as long as it is placed in a content section.
        var isInContent = await _context.DataBlockVersionLinks.AnyAsync(link =>
            link.DataBlockVersionId == dataBlockVersion.Id
        );

        return !isInContent
            && await _context
                .KeyStatisticsDataBlock.Where(ks => ks.ReleaseVersionId == releaseVersionId)
                .AllAsync(ks => ks.DataBlockVersionId != dataBlockVersion.Id);
    }

    public async Task<List<DataBlockVersion>> ListDataBlocks(Guid releaseVersionId)
    {
        return await _context
            .DataBlockVersions.Where(dataBlockVersion => dataBlockVersion.ReleaseVersionId == releaseVersionId)
            .ToListAsync();
    }
}
