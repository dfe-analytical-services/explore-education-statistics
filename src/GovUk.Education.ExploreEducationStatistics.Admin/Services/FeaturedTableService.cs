#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class FeaturedTableService : IFeaturedTableService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
    private readonly IDataBlockService _dataBlockService;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public FeaturedTableService(ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IDataBlockService dataBlockService,
        IUserService userService,
        IMapper mapper)
    {
        _contentDbContext = contentDbContext;
        _persistenceHelper = persistenceHelper;
        _dataBlockService = dataBlockService;
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<Either<ActionResult, FeaturedTableViewModel>> Get(Guid releaseId, Guid dataBlockId)
    {
        return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
            .OnSuccess(_userService.CheckCanViewRelease)
            .OnSuccess(async _ =>
                await _persistenceHelper.CheckEntityExists<FeaturedTable>(query =>
                    query.Where(ft => ft.DataBlockId == dataBlockId && ft.ReleaseId == releaseId)))
            .OnSuccess(featuredTable => _mapper.Map<FeaturedTableViewModel>(featuredTable));
    }

    public async Task<Either<ActionResult, List<FeaturedTableViewModel>>> List(Guid releaseId)
    {
        return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
            .OnSuccess(_userService.CheckCanViewRelease)
            .OnSuccess(async release =>
            {
                var featuredTableList = await ListFeaturedTables(release.Id);
                return _mapper.Map<List<FeaturedTableViewModel>>(featuredTableList);
            });
    }

    public async Task<Either<ActionResult, FeaturedTableViewModel>> Create(Guid releaseId,
        FeaturedTableCreateRequest request)
    {
        return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
            .OnSuccess(_userService.CheckCanUpdateRelease)
            .OnSuccess(async _ =>
                await _persistenceHelper.CheckEntityExists<DataBlock>(request.DataBlockId))
            .OnSuccessDo(async dataBlock =>
            {
                var hasFeaturedTable = await _contentDbContext.FeaturedTables
                    .AnyAsync(ft => ft.DataBlockId == dataBlock.Id);
                if (hasFeaturedTable)
                {
                    return new Either<ActionResult, Unit>(
                        ValidationUtils.ValidationActionResult(DataBlockAlreadyHasFeaturedTable));
                }

                return Unit.Instance;
            })
            .OnSuccess(async _ =>
            {
                var featuredTable = _mapper.Map<FeaturedTable>(request);
                featuredTable.ReleaseId = releaseId;
                featuredTable.CreatedById = _userService.GetUserId();

                var featuredTableList = await ListFeaturedTables(releaseId);

                var currentMaxOrder = featuredTableList
                    .Select(ft => ft.Order)
                    .Max(order => (int?)order);

                featuredTable.Order = currentMaxOrder.HasValue ? currentMaxOrder.Value + 1 : 0;

                await _contentDbContext.FeaturedTables.AddAsync(featuredTable);
                await _contentDbContext.SaveChangesAsync();

                return _mapper.Map<FeaturedTableViewModel>(featuredTable);
            });
    }

    public async Task<Either<ActionResult, FeaturedTableViewModel>> Update(
        Guid releaseId,
        Guid dataBlockId,
        FeaturedTableUpdateRequest request)
    {
        return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
            .OnSuccess(_userService.CheckCanUpdateRelease)
            .OnSuccess(async _ =>
                await _persistenceHelper.CheckEntityExists<FeaturedTable>(query =>
                    query.Where(ft => ft.DataBlockId == dataBlockId && ft.ReleaseId == releaseId)))
            .OnSuccess(async featuredTable =>
            {
                _contentDbContext.FeaturedTables.Update(featuredTable);

                featuredTable.Name = request.Name;
                featuredTable.Description = request.Description;

                await _contentDbContext.SaveChangesAsync();

                return _mapper.Map<FeaturedTableViewModel>(featuredTable);
            });
    }

    public async Task<Either<ActionResult, Unit>> Delete(Guid releaseId, Guid dataBlockId)
    {
        return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
            .OnSuccess(_userService.CheckCanUpdateRelease)
            .OnSuccess(async _ =>
                await _persistenceHelper.CheckEntityExists<FeaturedTable>(query =>
                    query.Where(ft => ft.DataBlockId == dataBlockId && ft.ReleaseId == releaseId)))
            .OnSuccessVoid(async featuredTable =>
            {
                _contentDbContext.FeaturedTables.Remove(featuredTable);
                await _contentDbContext.SaveChangesAsync();
            });
    }

    public async Task<Either<ActionResult, List<FeaturedTableViewModel>>> Reorder(
        Guid releaseId,
        List<Guid> newOrder)
    {
        return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
            .OnSuccess(_userService.CheckCanUpdateRelease)
            .OnSuccess(async release => await ListFeaturedTables(release.Id))
            .OnSuccess(async featuredTableList =>
            {
                if (!ComparerUtils.SequencesAreEqualIgnoringOrder(
                        newOrder,
                        featuredTableList.Select(ft => ft.Id)))
                {
                    return new Either<ActionResult, List<FeaturedTable>>(ValidationUtils.ValidationActionResult(
                        ProvidedFeaturedTableIdsDifferFromReleaseFeaturedTableIds));
                }

                var updatingUserId = _userService.GetUserId();

                newOrder.ForEach((featuredTableId, order) =>
                {
                    var matchingFeaturedTable = featuredTableList.Single(ft => ft.Id == featuredTableId);
                    matchingFeaturedTable.Order = order;
                    matchingFeaturedTable.UpdatedById = updatingUserId;
                });

                _contentDbContext.FeaturedTables.UpdateRange(featuredTableList);
                await _contentDbContext.SaveChangesAsync();

                return featuredTableList;
            })
            .OnSuccess(featuredTables =>
                _mapper.Map<List<FeaturedTableViewModel>>(
                    featuredTables.OrderBy(ft => ft.Order)));
    }

    private async Task<List<FeaturedTable>> ListFeaturedTables(Guid releaseId)
    {
        var releaseDataBlockIds = await _contentDbContext
            .ReleaseContentBlocks
            .Where(releaseContentBlock => releaseContentBlock.ReleaseId == releaseId)
            .Select(releaseContentBlock => releaseContentBlock.ContentBlock)
            .OfType<DataBlock>()
            .Select(dataBlock => dataBlock.Id)
            .ToListAsync();

        return await _contentDbContext.FeaturedTables
            .Where(ft => releaseDataBlockIds.Contains(ft.DataBlockId))
            .OrderBy(ft => ft.Order)
            .ThenBy(ft => ft.Name)
            .ToListAsync();
    }
}
