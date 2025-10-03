#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
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
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;

public class KeyStatisticService : IKeyStatisticService
{
    private readonly ContentDbContext _context;
    private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
    private readonly IDataBlockService _dataBlockService;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public KeyStatisticService(
        ContentDbContext context,
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IDataBlockService dataBlockService,
        IUserService userService,
        IMapper mapper
    )
    {
        _context = context;
        _persistenceHelper = persistenceHelper;
        _dataBlockService = dataBlockService;
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<Either<ActionResult, KeyStatisticDataBlockViewModel>> CreateKeyStatisticDataBlock(
        Guid releaseVersionId,
        KeyStatisticDataBlockCreateRequest request
    )
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(_ =>
                _persistenceHelper.CheckEntityExists<DataBlockVersion>(query =>
                    query.Where(dataBlockVersion =>
                        dataBlockVersion.Id == request.DataBlockId
                        && dataBlockVersion.ReleaseVersionId == releaseVersionId
                    )
                )
            )
            .OnSuccessDo(async dataBlockVersion =>
                await _dataBlockService.IsUnattachedDataBlock(releaseVersionId, dataBlockVersion)
                    ? new Either<ActionResult, Unit>(Unit.Instance)
                    : ValidationActionResult(DataBlockShouldBeUnattached)
            )
            .OnSuccess(async dataBlockVersion =>
            {
                var keyStatisticDataBlock = _mapper.Map<KeyStatisticDataBlock>(request);
                keyStatisticDataBlock.ReleaseVersionId = releaseVersionId;
                keyStatisticDataBlock.CreatedById = _userService.GetUserId();
                keyStatisticDataBlock.DataBlockParentId = dataBlockVersion.DataBlockParentId;

                var currentMaxOrder = await _context
                    .KeyStatistics.Where(ks => ks.ReleaseVersionId == releaseVersionId)
                    .Select(ks => ks.Order)
                    .MaxAsync(order => (int?)order);
                keyStatisticDataBlock.Order = currentMaxOrder.HasValue ? currentMaxOrder.Value + 1 : 0;

                await _context.KeyStatisticsDataBlock.AddAsync(keyStatisticDataBlock);
                await _context.SaveChangesAsync();

                return _mapper.Map<KeyStatisticDataBlockViewModel>(keyStatisticDataBlock);
            });
    }

    public async Task<Either<ActionResult, KeyStatisticTextViewModel>> CreateKeyStatisticText(
        Guid releaseVersionId,
        KeyStatisticTextCreateRequest request
    )
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async _ =>
            {
                var keyStatisticText = _mapper.Map<KeyStatisticText>(request);
                keyStatisticText.ReleaseVersionId = releaseVersionId;
                keyStatisticText.CreatedById = _userService.GetUserId();

                var currentMaxOrder = await _context
                    .KeyStatistics.Where(ks => ks.ReleaseVersionId == releaseVersionId)
                    .Select(ks => ks.Order)
                    .MaxAsync(order => (int?)order);
                keyStatisticText.Order = currentMaxOrder.HasValue ? currentMaxOrder.Value + 1 : 0;

                await _context.KeyStatisticsText.AddAsync(keyStatisticText);
                await _context.SaveChangesAsync();

                return _mapper.Map<KeyStatisticTextViewModel>(keyStatisticText);
            });
    }

    public async Task<Either<ActionResult, KeyStatisticDataBlockViewModel>> UpdateKeyStatisticDataBlock(
        Guid releaseVersionId,
        Guid keyStatisticId,
        KeyStatisticDataBlockUpdateRequest request
    )
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
                await _persistenceHelper.CheckEntityExists<KeyStatisticDataBlock>(
                    keyStatisticId,
                    query => query.Where(keyStat => keyStat.ReleaseVersionId == releaseVersion.Id)
                )
            )
            .OnSuccess(async keyStat =>
            {
                _context.KeyStatisticsDataBlock.Update(keyStat);

                keyStat.UpdatedById = _userService.GetUserId();
                keyStat.Trend = request.Trend;
                keyStat.GuidanceTitle = request.GuidanceTitle;
                keyStat.GuidanceText = request.GuidanceText;

                await _context.SaveChangesAsync();

                return _mapper.Map<KeyStatisticDataBlockViewModel>(keyStat);
            });
    }

    public async Task<Either<ActionResult, KeyStatisticTextViewModel>> UpdateKeyStatisticText(
        Guid releaseVersionId,
        Guid keyStatisticId,
        KeyStatisticTextUpdateRequest request
    )
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
                await _persistenceHelper.CheckEntityExists<KeyStatisticText>(
                    keyStatisticId,
                    query => query.Where(keyStat => keyStat.ReleaseVersionId == releaseVersion.Id)
                )
            )
            .OnSuccess(async keyStat =>
            {
                _context.KeyStatisticsText.Update(keyStat);

                keyStat.UpdatedById = _userService.GetUserId();
                keyStat.Title = request.Title;
                keyStat.Statistic = request.Statistic;
                keyStat.Trend = request.Trend;
                keyStat.GuidanceTitle = request.GuidanceTitle;
                keyStat.GuidanceText = request.GuidanceText;

                await _context.SaveChangesAsync();

                return _mapper.Map<KeyStatisticTextViewModel>(keyStat);
            });
    }

    public async Task<Either<ActionResult, Unit>> Delete(Guid releaseVersionId, Guid keyStatisticId)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
                await _persistenceHelper.CheckEntityExists<KeyStatistic>(query =>
                    query.Where(keyStat =>
                        keyStat.Id == keyStatisticId && keyStat.ReleaseVersionId == releaseVersion.Id
                    )
                )
            )
            .OnSuccessVoid(async keyStat =>
            {
                _context.KeyStatistics.Remove(keyStat);
                await _context.SaveChangesAsync();
            });
    }

    public async Task<Either<ActionResult, List<KeyStatisticViewModel>>> Reorder(
        Guid releaseVersionId,
        List<Guid> newOrder
    )
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess<ActionResult, ReleaseVersion, List<KeyStatisticViewModel>>(async release =>
            {
                var keyStatistics = await _context
                    .KeyStatistics.Where(ks => ks.ReleaseVersionId == release.Id)
                    .ToListAsync();

                if (!ComparerUtils.SequencesAreEqualIgnoringOrder(newOrder, keyStatistics.Select(ks => ks.Id)))
                {
                    return ValidationActionResult(ProvidedKeyStatIdsDifferFromReleaseKeyStatIds);
                }

                var updatingUserId = _userService.GetUserId();

                newOrder.ForEach(
                    (keyStatisticId, order) =>
                    {
                        var matchingKeyStat = keyStatistics.Single(ks => ks.Id == keyStatisticId);
                        matchingKeyStat.Order = order;
                        matchingKeyStat.UpdatedById = updatingUserId;
                    }
                );

                _context.KeyStatistics.UpdateRange(keyStatistics);
                await _context.SaveChangesAsync();

                return _mapper.Map<List<KeyStatisticViewModel>>(keyStatistics.OrderBy(ks => ks.Order));
            });
    }
}
