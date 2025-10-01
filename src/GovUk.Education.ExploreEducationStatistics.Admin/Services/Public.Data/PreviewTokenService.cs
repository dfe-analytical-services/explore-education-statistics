#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

public class PreviewTokenService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext,
    IUserService userService
) : IPreviewTokenService
{
    public async Task<Either<ActionResult, PreviewTokenViewModel>> CreatePreviewToken(
        Guid dataSetVersionId,
        string label,
        DateTimeOffset? activates,
        DateTimeOffset? expiry,
        CancellationToken cancellationToken = default)
    {
        return await userService.CheckIsBauUser()
            .OnSuccess(async () => await CheckDataSetVersionExists(dataSetVersionId, cancellationToken))
            .OnSuccessDo(ValidateDraftDataSetVersion)
            .OnSuccess(async () =>
            {
                activates ??= DateTimeOffset.UtcNow;
                var previewToken = publicDataDbContext.PreviewTokens.Add(new PreviewToken
                {
                    DataSetVersionId = dataSetVersionId,
                    Label = label,
                    Created = DateTimeOffset.UtcNow,
                    Activates = activates.Value,
                    Expires = expiry ?? activates.Value.AddDays(7),
                    CreatedByUserId = userService.GetUserId()
                });
                await publicDataDbContext.SaveChangesAsync(cancellationToken);
                return await MapPreviewToken(previewToken.Entity);
            });
    }

    public async Task<Either<ActionResult, PreviewTokenViewModel>> GetPreviewToken(
        Guid previewTokenId,
        CancellationToken cancellationToken = default)
    {
        return await userService.CheckIsBauUser()
            .OnSuccess(async () => await publicDataDbContext.PreviewTokens
                .AsNoTracking()
                .SingleOrNotFoundAsync(pt => pt.Id == previewTokenId, cancellationToken: cancellationToken))
            .OnSuccess(MapPreviewToken);
    }

    public async Task<Either<ActionResult, IReadOnlyList<PreviewTokenViewModel>>> ListPreviewTokens(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        return await userService.CheckIsBauUser()
            .OnSuccess(async () => await CheckDataSetVersionExists(dataSetVersionId, cancellationToken))
            .OnSuccess(async () => await DoList(dataSetVersionId, cancellationToken));
    }

    public async Task<Either<ActionResult, PreviewTokenViewModel>> RevokePreviewToken(
        Guid previewTokenId,
        CancellationToken cancellationToken = default)
    {
        return await userService.CheckIsBauUser()
            .OnSuccess(async () => await publicDataDbContext.PreviewTokens
                .SingleOrNotFoundAsync(pt => pt.Id == previewTokenId, cancellationToken: cancellationToken))
            .OnSuccessDo(ValidatePreviewToken)
            .OnSuccess(async previewToken =>
            {
                previewToken.Expires = DateTimeOffset.UtcNow;
                await publicDataDbContext.SaveChangesAsync(cancellationToken);
                return await MapPreviewToken(previewToken);
            });
    }

    private async Task<Either<ActionResult, DataSetVersion>> CheckDataSetVersionExists(
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext.DataSetVersions
            .AsNoTracking()
            .SingleOrNotFoundAsync(dsv => dsv.Id == dataSetVersionId, cancellationToken);
    }

    private static Either<ActionResult, Unit> ValidateDraftDataSetVersion(DataSetVersion dataSetVersion)
    {
        return dataSetVersion.Status != DataSetVersionStatus.Draft
            ? ValidationUtils.ValidationResult(new ErrorViewModel
            {
                Code = ValidationMessages.DataSetVersionStatusNotDraft.Code,
                Message = ValidationMessages.DataSetVersionStatusNotDraft.Message,
                Path = nameof(PreviewTokenCreateRequest.DataSetVersionId).ToLowerFirst(),
                Detail = new InvalidErrorDetail<Guid>(dataSetVersion.Id)
            })
            : Unit.Instance;
    }

    private static Either<ActionResult, Unit> ValidatePreviewToken(PreviewToken previewToken)
    {
        return previewToken.Status is PreviewTokenStatus.Expired
            ? ValidationUtils.ValidationResult(new ErrorViewModel
            {
                Code = ValidationMessages.PreviewTokenExpired.Code,
                Message = ValidationMessages.PreviewTokenExpired.Message,
                Path = "previewTokenId",
                Detail = new InvalidErrorDetail<Guid>(previewToken.Id)
            })
            : Unit.Instance;
    }

    private async Task<Either<ActionResult, IReadOnlyList<PreviewTokenViewModel>>> DoList(
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        var previewTokens = await publicDataDbContext
            .PreviewTokens
            .AsNoTracking()
            .Where(pt => pt.DataSetVersionId == dataSetVersionId)
            .ToListAsync(cancellationToken);

        return await previewTokens
            .ToAsyncEnumerable()
            .SelectAwait(async pt => await MapPreviewToken(pt))
            .OrderByDescending(pt => pt.Expiry)
            .ToListAsync(cancellationToken);
    }

    private async Task<PreviewTokenViewModel> MapPreviewToken(PreviewToken previewToken)
    {
        var createdByEmail = await contentDbContext.Users
            .Where(u => u.Id == previewToken.CreatedByUserId)
            .Select(u => u.Email)
            .SingleAsync();

        return new PreviewTokenViewModel
        {
            Id = previewToken.Id,
            Label = previewToken.Label,
            Status = previewToken.Status,
            CreatedByEmail = createdByEmail,
            Created = previewToken.Created,
            Activates = previewToken.Activates,
            Expiry = previewToken.Expires,
            Updated = previewToken.Updated
        };
    }
}
