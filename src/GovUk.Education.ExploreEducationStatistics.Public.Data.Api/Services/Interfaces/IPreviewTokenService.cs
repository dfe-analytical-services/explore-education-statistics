namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IPreviewTokenService
{
    Task<bool> ValidatePreviewTokenForDataSet(
        string previewToken,
        Guid dataSetId,
        CancellationToken cancellationToken = default);
    
    Task<bool> ValidatePreviewTokenForDataSetVersion(
        string previewToken,
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default);
}
