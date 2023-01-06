#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;

public class KeyStatisticService : IKeyStatisticService
{
    private readonly ContentDbContext _context;
    private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public KeyStatisticService(
        ContentDbContext context,
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IUserService userService,
        IMapper mapper)
    {
        _context = context;
        _persistenceHelper = persistenceHelper;
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<Either<ActionResult, KeyStatisticDataBlockViewModel>> CreateKeyStatisticDataBlock(
        Guid releaseId,
        KeyStatisticDataBlockCreateRequest request)
    {
        return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
            .OnSuccess(_userService.CheckCanUpdateRelease)
            .OnSuccess(async _ =>
                await _persistenceHelper.CheckEntityExists<DataBlock>(request.DataBlockId))
            .OnSuccess(dataBlock =>
            {
                if (dataBlock.ContentSectionId != null)
                {
                    throw new ArgumentException("Data block shouldn't be attached to a content section");
                }

                return dataBlock;
            })
            .OnSuccess(async _ =>
            {
                var keyStatisticDataBlock = _mapper.Map<KeyStatisticDataBlock>(request);
                keyStatisticDataBlock.ReleaseId = releaseId;

                var orderList = await _context.KeyStatistics
                    .Where(ks => ks.ReleaseId == releaseId)
                    .Select(ks => ks.Order)
                    .ToListAsync();
                keyStatisticDataBlock.Order = orderList.IsNullOrEmpty() ? 0 : orderList.Max() + 1;

                await _context.KeyStatisticsDataBlock.AddAsync(keyStatisticDataBlock);
                await _context.SaveChangesAsync();

                return _mapper.Map<KeyStatisticDataBlockViewModel>(keyStatisticDataBlock);
            });
    }

    public async Task<Either<ActionResult, KeyStatisticDataBlockViewModel>> UpdateKeyStatisticDataBlock(
        Guid releaseId,
        Guid keyStatisticId,
        KeyStatisticDataBlockUpdateRequest request)
    {
        return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
            .OnSuccess(_userService.CheckCanUpdateRelease)
            .OnSuccess(async release =>
                await _persistenceHelper.CheckEntityExists<KeyStatisticDataBlock>(keyStatisticId, query =>
                    query.Where(keyStat => keyStat.ReleaseId == release.Id)))
            .OnSuccess(async keyStat =>
            {
                _context.Update(keyStat);

                keyStat.Trend = request.Trend;
                keyStat.GuidanceTitle = request.GuidanceTitle;
                keyStat.GuidanceText = request.GuidanceText;

                await _context.SaveChangesAsync();

                return _mapper.Map<KeyStatisticDataBlockViewModel>(keyStat);
            });
    }

    public async Task<Either<ActionResult, Unit>> Delete(Guid releaseId, Guid keyStatisticId)
    {
        return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
            .OnSuccess(_userService.CheckCanUpdateRelease)
            .OnSuccess(async release =>
                await _persistenceHelper.CheckEntityExists<KeyStatistic>(keyStatisticId, query =>
                    query
                        .Include(keyStat => (keyStat as KeyStatisticDataBlock)!.DataBlock) // TODO Include can be removed in EES-3988
                        .Where(keyStat => keyStat.ReleaseId == release.Id)))
            // NOTE: Ensure old key stats created before the new key stat work was deployed
            // become available for selection again by setting ContentSectionId to null
            // TODO: Remove this in EES-3988
            .OnSuccess(async keyStat =>
            {
                if (keyStat.GetType() == typeof(KeyStatisticDataBlock))
                {
                    var dataBlock = (keyStat as KeyStatisticDataBlock)!.DataBlock;
                    dataBlock.ContentSection = null;
                    dataBlock.ContentSectionId = null;
                    _context.ContentBlocks.Update(dataBlock);
                    await _context.SaveChangesAsync();
                }
                return keyStat;
            })
            .OnSuccessVoid(async keyStat =>
            {
                _context.KeyStatistics.Remove(keyStat);
                await _context.SaveChangesAsync();
            });
    }

    public async Task DeleteAnyAssociatedWithDataBlock(Guid releaseId, Guid dataBlockId)
    {
        var keyStats = await _context
            .KeyStatisticsDataBlock
            .Where(ks => ks.ReleaseId == releaseId && ks.DataBlockId == dataBlockId)
            .ToListAsync();
        _context.KeyStatisticsDataBlock.RemoveRange(keyStats);
        await _context.SaveChangesAsync();
    }

    public async Task<Either<ActionResult, List<KeyStatisticViewModel>>> Reorder(Guid releaseId, Dictionary<Guid, int> newKeyStatisticOrder)
    {
        return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
            .OnSuccess(_userService.CheckCanUpdateRelease)
            .OnSuccess(async release =>
            {
                var keyStatistics = _context.KeyStatistics
                    .Where(ks => ks.ReleaseId == release.Id)
                    .ToList();

                var idOrderPairList = newKeyStatisticOrder.ToList();

                if (keyStatistics.Count != idOrderPairList.Count)
                {
                    throw new ArgumentException("newKeyStatisticOrder.Count must equal release's key statistics count");
                }

                idOrderPairList.ForEach(idOrderPair =>
                {
                    var (keyStatisticId, newOrder) = idOrderPair;
                    var matchingKeyStat = keyStatistics.Find(ks => ks.Id == keyStatisticId);
                    if (matchingKeyStat != null)
                    {
                        matchingKeyStat.Order = newOrder;
                    }
                });

                _context.UpdateRange(keyStatistics);
                await _context.SaveChangesAsync();

                return _mapper.Map<List<KeyStatisticViewModel>>(
                    keyStatistics.OrderBy(ks => ks.Order));
            });
    }
}
