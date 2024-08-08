namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IPreviewTokenService
{
    Task<bool> ValidatePreviewToken(
        string previewToken,
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default);
}
