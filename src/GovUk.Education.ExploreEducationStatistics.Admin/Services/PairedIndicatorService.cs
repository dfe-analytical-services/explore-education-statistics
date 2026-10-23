#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class PairedIndicatorService(
    ContentDbContext contentDbContext,
    StatisticsDbContext statisticsDbContext,
    IUserService userService,
    IDataImportService dataImportService,
    IPrivateBlobCacheService privateBlobCacheService
) : IPairedIndicatorService
{
    public async Task<Either<ActionResult, PairedIndicatorsViewModel>> GetPairedIndicators(
        Guid releaseVersionId,
        Guid subjectId,
        CancellationToken cancellationToken = default
    )
    {
        return await FindDataReleaseFile(releaseVersionId, subjectId, cancellationToken)
            .OnSuccessDo(releaseFile => userService.CheckCanViewReleaseVersion(releaseFile.ReleaseVersion))
            .OnSuccess(releaseFile => PairedIndicatorsViewModel.Create(releaseFile.PairedIndicators));
    }

    public async Task<Either<ActionResult, PairedIndicatorsViewModel>> UpdatePairedIndicators(
        Guid releaseVersionId,
        Guid subjectId,
        PairedIndicatorsUpdateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        return await FindDataReleaseFile(releaseVersionId, subjectId, cancellationToken)
            .OnSuccessDo(releaseFile => userService.CheckCanUpdateReleaseVersion(releaseFile.ReleaseVersion))
            .OnSuccessDo(releaseFile => CheckImportComplete(releaseFile.FileId))
            .OnSuccessDo(() => CheckIndicatorsBelongToSubject(subjectId, request, cancellationToken))
            .OnSuccess(async releaseFile =>
            {
                // The list is reassigned rather than modified in place, as the column has no value comparer
                // and EF would otherwise not detect the change
                releaseFile.PairedIndicators =
                    request.PairedIndicators.Count == 0
                        ? null
                        : request
                            .PairedIndicators.Select(pair => new PairedIndicatorEntry(
                                ParentIndicatorId: pair.ParentIndicatorId,
                                ChildIndicatorId: pair.ChildIndicatorId
                            ))
                            .ToList();

                await contentDbContext.SaveChangesAsync(cancellationToken);

                await privateBlobCacheService.DeleteItemAsync(
                    new PrivateSubjectMetaCacheKey(releaseVersionId: releaseVersionId, subjectId: subjectId)
                );

                return PairedIndicatorsViewModel.Create(releaseFile.PairedIndicators);
            });
    }

    private Task<Either<ActionResult, ReleaseFile>> FindDataReleaseFile(
        Guid releaseVersionId,
        Guid subjectId,
        CancellationToken cancellationToken
    )
    {
        return contentDbContext
            .ReleaseFiles.Include(rf => rf.ReleaseVersion)
                .ThenInclude(rv => rv.Release)
            .Include(rf => rf.File)
            .SingleOrNotFoundAsync(
                rf =>
                    rf.ReleaseVersionId == releaseVersionId
                    && rf.File.SubjectId == subjectId
                    && rf.File.Type == FileType.Data,
                cancellationToken
            );
    }

    private async Task<Either<ActionResult, Unit>> CheckImportComplete(Guid fileId)
    {
        var import = await dataImportService.GetImport(fileId);

        return import?.Status == DataImportStatus.COMPLETE
            ? Unit.Instance
            : ValidationResult(ToErrorViewModel(ValidationMessages.PairedIndicatorsImportNotComplete));
    }

    private async Task<Either<ActionResult, Unit>> CheckIndicatorsBelongToSubject(
        Guid subjectId,
        PairedIndicatorsUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        var requestedIndicatorIds = request
            .PairedIndicators.SelectMany(pair => new[] { pair.ParentIndicatorId, pair.ChildIndicatorId })
            .ToHashSet();

        if (requestedIndicatorIds.Count == 0)
        {
            return Unit.Instance;
        }

        var matchingIndicatorCount = await statisticsDbContext
            .Indicator.Where(indicator =>
                indicator.IndicatorGroup.SubjectId == subjectId && requestedIndicatorIds.Contains(indicator.Id)
            )
            .CountAsync(cancellationToken);

        return matchingIndicatorCount == requestedIndicatorIds.Count
            ? Unit.Instance
            : ValidationResult(ToErrorViewModel(ValidationMessages.PairedIndicatorsIndicatorNotForSubject));
    }

    private static ErrorViewModel ToErrorViewModel(LocalizableMessage message) =>
        new() { Code = message.Code, Message = message.Message };
}
