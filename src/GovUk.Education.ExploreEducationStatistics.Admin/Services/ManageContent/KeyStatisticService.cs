#nullable enable
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

public class KeyStatisticService(
    ContentDbContext context,
    IPersistenceHelper<ContentDbContext> persistenceHelper,
    IDataBlockService dataBlockService,
    IUserService userService
) : IKeyStatisticService
{
    public async Task<Either<ActionResult, KeyStatisticDataBlockViewModel>> CreateKeyStatisticDataBlock(
        Guid releaseVersionId,
        KeyStatisticDataBlockCreateRequest request
    ) =>
        await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(_ =>
                persistenceHelper.CheckEntityExists<DataBlockVersion>(query =>
                    query.Where(dataBlockVersion =>
                        dataBlockVersion.Id == request.DataBlockId
                        && dataBlockVersion.ReleaseVersionId == releaseVersionId
                    )
                )
            )
            .OnSuccessDo(async dataBlockVersion =>
                await dataBlockService.IsUnattachedDataBlock(releaseVersionId, dataBlockVersion)
                    ? new Either<ActionResult, Unit>(Unit.Instance)
                    : ValidationActionResult(DataBlockShouldBeUnattached)
            )
            .OnSuccess(async dataBlockVersion =>
            {
                var currentMaxOrder = await context
                    .KeyStatistics.Where(ks => ks.ReleaseVersionId == releaseVersionId)
                    .Select(ks => ks.Order)
                    .MaxAsync(order => (int?)order);

                var keyStatisticDataBlock = new KeyStatisticDataBlock
                {
                    DataBlockId = request.DataBlockId,
                    DataBlockParentId = dataBlockVersion.DataBlockParentId,
                    ReleaseVersionId = releaseVersionId,
                    Trend = request.Trend,
                    GuidanceTitle = request.GuidanceTitle,
                    GuidanceText = request.GuidanceText,
                    Order = currentMaxOrder.HasValue ? currentMaxOrder.Value + 1 : 0,
                    CreatedById = userService.GetUserId(),
                };

                await context.KeyStatisticsDataBlock.AddAsync(keyStatisticDataBlock);
                await context.SaveChangesAsync();

                return KeyStatisticDataBlockViewModel.FromKeyStatisticDataBlock(keyStatisticDataBlock);
            });

    public async Task<Either<ActionResult, KeyStatisticTextViewModel>> CreateKeyStatisticText(
        Guid releaseVersionId,
        KeyStatisticTextCreateRequest request
    ) =>
        await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async _ =>
            {
                var currentMaxOrder = await context
                    .KeyStatistics.Where(ks => ks.ReleaseVersionId == releaseVersionId)
                    .Select(ks => ks.Order)
                    .MaxAsync(order => (int?)order);

                var keyStatisticText = new KeyStatisticText
                {
                    ReleaseVersionId = releaseVersionId,
                    Title = request.Title,
                    Statistic = request.Statistic,
                    Trend = request.Trend,
                    GuidanceTitle = request.GuidanceTitle,
                    GuidanceText = request.GuidanceText,
                    Order = currentMaxOrder.HasValue ? currentMaxOrder.Value + 1 : 0,
                    CreatedById = userService.GetUserId(),
                };

                await context.KeyStatisticsText.AddAsync(keyStatisticText);
                await context.SaveChangesAsync();

                return KeyStatisticTextViewModel.FromKeyStatisticText(keyStatisticText);
            });

    public async Task<Either<ActionResult, KeyStatisticDataBlockViewModel>> UpdateKeyStatisticDataBlock(
        Guid releaseVersionId,
        Guid keyStatisticId,
        KeyStatisticDataBlockUpdateRequest request
    ) =>
        await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
                await persistenceHelper.CheckEntityExists<KeyStatisticDataBlock>(
                    keyStatisticId,
                    query => query.Where(keyStat => keyStat.ReleaseVersionId == releaseVersion.Id)
                )
            )
            .OnSuccess(async keyStat =>
            {
                context.KeyStatisticsDataBlock.Update(keyStat);

                keyStat.UpdatedById = userService.GetUserId();
                keyStat.Trend = request.Trend;
                keyStat.GuidanceTitle = request.GuidanceTitle;
                keyStat.GuidanceText = request.GuidanceText;

                await context.SaveChangesAsync();

                return KeyStatisticDataBlockViewModel.FromKeyStatisticDataBlock(keyStat);
            });

    public async Task<Either<ActionResult, KeyStatisticTextViewModel>> UpdateKeyStatisticText(
        Guid releaseVersionId,
        Guid keyStatisticId,
        KeyStatisticTextUpdateRequest request
    ) =>
        await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
                await persistenceHelper.CheckEntityExists<KeyStatisticText>(
                    keyStatisticId,
                    query => query.Where(keyStat => keyStat.ReleaseVersionId == releaseVersion.Id)
                )
            )
            .OnSuccess(async keyStat =>
            {
                context.KeyStatisticsText.Update(keyStat);

                keyStat.UpdatedById = userService.GetUserId();
                keyStat.Title = request.Title;
                keyStat.Statistic = request.Statistic;
                keyStat.Trend = request.Trend;
                keyStat.GuidanceTitle = request.GuidanceTitle;
                keyStat.GuidanceText = request.GuidanceText;

                await context.SaveChangesAsync();

                return KeyStatisticTextViewModel.FromKeyStatisticText(keyStat);
            });

    public async Task<Either<ActionResult, Unit>> Delete(Guid releaseVersionId, Guid keyStatisticId) =>
        await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
                await persistenceHelper.CheckEntityExists<KeyStatistic>(query =>
                    query.Where(keyStat =>
                        keyStat.Id == keyStatisticId && keyStat.ReleaseVersionId == releaseVersion.Id
                    )
                )
            )
            .OnSuccessVoid(async keyStat =>
            {
                context.KeyStatistics.Remove(keyStat);
                await context.SaveChangesAsync();
            });

    public async Task<Either<ActionResult, List<KeyStatisticViewModel>>> Reorder(
        Guid releaseVersionId,
        List<Guid> newOrder
    ) =>
        await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess<ActionResult, ReleaseVersion, List<KeyStatisticViewModel>>(async release =>
            {
                var keyStatistics = await context
                    .KeyStatistics.Where(ks => ks.ReleaseVersionId == release.Id)
                    .ToListAsync();

                if (!ComparerUtils.SequencesAreEqualIgnoringOrder(newOrder, keyStatistics.Select(ks => ks.Id)))
                {
                    return ValidationActionResult(ProvidedKeyStatIdsDifferFromReleaseKeyStatIds);
                }

                var updatingUserId = userService.GetUserId();

                newOrder.ForEach(
                    (keyStatisticId, order) =>
                    {
                        var matchingKeyStat = keyStatistics.Single(ks => ks.Id == keyStatisticId);
                        matchingKeyStat.Order = order;
                        matchingKeyStat.UpdatedById = updatingUserId;
                    }
                );

                context.KeyStatistics.UpdateRange(keyStatistics);
                await context.SaveChangesAsync();

                return keyStatistics.OrderBy(ks => ks.Order).Select(KeyStatisticViewModel.FromKeyStatistic).ToList();
            });
}
