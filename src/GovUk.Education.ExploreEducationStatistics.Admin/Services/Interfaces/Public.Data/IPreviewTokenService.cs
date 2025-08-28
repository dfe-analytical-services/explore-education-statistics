#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;

public interface IPreviewTokenService
{
    Task<Either<ActionResult, PreviewTokenViewModel>> CreatePreviewToken(
        Guid dataSetVersionId,
        string label,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, PreviewTokenViewModel>> GetPreviewToken(
        Guid previewTokenId,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, IReadOnlyList<PreviewTokenViewModel>>> ListPreviewTokens(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, PreviewTokenViewModel>> RevokePreviewToken(
        Guid previewTokenId,
        CancellationToken cancellationToken = default);
}
