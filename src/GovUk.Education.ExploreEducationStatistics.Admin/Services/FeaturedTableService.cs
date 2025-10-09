#nullable enable
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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class FeaturedTableService : IFeaturedTableService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public FeaturedTableService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IUserService userService,
        IMapper mapper
    )
    {
        _contentDbContext = contentDbContext;
        _persistenceHelper = persistenceHelper;
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<Either<ActionResult, FeaturedTableViewModel>> Get(Guid releaseVersionId, Guid dataBlockId)
    {
        // @MarkFix this as an example to change maybe?
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccess(async _ =>
                await _persistenceHelper.CheckEntityExists<FeaturedTable>(query =>
                    query.Where(ft => ft.DataBlockId == dataBlockId && ft.ReleaseVersionId == releaseVersionId)
                )
            )
            .OnSuccess(_mapper.Map<FeaturedTableViewModel>);
    }

    public async Task<Either<ActionResult, List<FeaturedTableViewModel>>> List(Guid releaseVersionId)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                var featuredTableList = await ListFeaturedTables(releaseVersion.Id);
                return _mapper.Map<List<FeaturedTableViewModel>>(featuredTableList);
            });
    }

    public async Task<Either<ActionResult, FeaturedTableViewModel>> Create(
        Guid releaseVersionId,
        FeaturedTableCreateRequest request
    )
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async _ => await _persistenceHelper.CheckEntityExists<DataBlock>(request.DataBlockId))
            .OnSuccessDo(async dataBlock =>
            {
                var hasFeaturedTable = await _contentDbContext.FeaturedTables.AnyAsync(ft =>
                    ft.DataBlockId == dataBlock.Id
                );
                if (hasFeaturedTable)
                {
                    return new Either<ActionResult, Unit>(
                        ValidationUtils.ValidationActionResult(DataBlockAlreadyHasFeaturedTable)
                    );
                }

                return Unit.Instance;
            })
            .OnSuccess(async _ =>
            {
                var dataBlockParentId = _contentDbContext
                    .DataBlockVersions.First(dataBlockVersion => dataBlockVersion.Id == request.DataBlockId)
                    .DataBlockParentId;

                var featuredTable = _mapper.Map<FeaturedTable>(request);
                featuredTable.ReleaseVersionId = releaseVersionId;
                featuredTable.DataBlockParentId = dataBlockParentId;
                featuredTable.CreatedById = _userService.GetUserId();

                var featuredTableList = await ListFeaturedTables(releaseVersionId);

                var currentMaxOrder = featuredTableList.Select(ft => ft.Order).Max(order => (int?)order);

                featuredTable.Order = currentMaxOrder.HasValue ? currentMaxOrder.Value + 1 : 0;

                await _contentDbContext.FeaturedTables.AddAsync(featuredTable);
                await _contentDbContext.SaveChangesAsync();

                return _mapper.Map<FeaturedTableViewModel>(featuredTable);
            });
    }

    public async Task<Either<ActionResult, FeaturedTableViewModel>> Update(
        Guid releaseVersionId,
        Guid dataBlockId,
        FeaturedTableUpdateRequest request
    )
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async _ =>
                await _persistenceHelper.CheckEntityExists<FeaturedTable>(query =>
                    query.Where(ft => ft.DataBlockId == dataBlockId && ft.ReleaseVersionId == releaseVersionId)
                )
            )
            .OnSuccess(async featuredTable =>
            {
                _contentDbContext.FeaturedTables.Update(featuredTable);

                featuredTable.Name = request.Name;
                featuredTable.Description = request.Description;

                await _contentDbContext.SaveChangesAsync();

                return _mapper.Map<FeaturedTableViewModel>(featuredTable);
            });
    }

    public async Task<Either<ActionResult, Unit>> Delete(Guid releaseVersionId, Guid dataBlockId)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async _ =>
                await _persistenceHelper.CheckEntityExists<FeaturedTable>(query =>
                    query.Where(ft => ft.DataBlockId == dataBlockId && ft.ReleaseVersionId == releaseVersionId)
                )
            )
            .OnSuccessVoid(async featuredTable =>
            {
                _contentDbContext.FeaturedTables.Remove(featuredTable);
                await _contentDbContext.SaveChangesAsync();
            });
    }

    public async Task<Either<ActionResult, List<FeaturedTableViewModel>>> Reorder(
        Guid releaseVersionId,
        List<Guid> newOrder
    )
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion => await ListFeaturedTables(releaseVersion.Id))
            .OnSuccess(async featuredTableList =>
            {
                if (!ComparerUtils.SequencesAreEqualIgnoringOrder(newOrder, featuredTableList.Select(ft => ft.Id)))
                {
                    return new Either<ActionResult, List<FeaturedTable>>(
                        ValidationUtils.ValidationActionResult(
                            ProvidedFeaturedTableIdsDifferFromReleaseFeaturedTableIds
                        )
                    );
                }

                var updatingUserId = _userService.GetUserId();

                newOrder.ForEach(
                    (featuredTableId, order) =>
                    {
                        var matchingFeaturedTable = featuredTableList.Single(ft => ft.Id == featuredTableId);
                        matchingFeaturedTable.Order = order;
                        matchingFeaturedTable.UpdatedById = updatingUserId;
                    }
                );

                _contentDbContext.FeaturedTables.UpdateRange(featuredTableList);
                await _contentDbContext.SaveChangesAsync();

                return featuredTableList;
            })
            .OnSuccess(featuredTables =>
                _mapper.Map<List<FeaturedTableViewModel>>(featuredTables.OrderBy(ft => ft.Order))
            );
    }

    private async Task<List<FeaturedTable>> ListFeaturedTables(Guid releaseVersionId)
    {
        var releaseDataBlockIds = await _contentDbContext
            .ContentBlocks.Where(block => block.ReleaseVersionId == releaseVersionId)
            .OfType<DataBlock>()
            .Select(dataBlock => dataBlock.Id)
            .ToListAsync();

        return await _contentDbContext
            .FeaturedTables.Where(ft => releaseDataBlockIds.Contains(ft.DataBlockId))
            .OrderBy(ft => ft.Order)
            .ThenBy(ft => ft.Name)
            .ToListAsync();
    }
}
