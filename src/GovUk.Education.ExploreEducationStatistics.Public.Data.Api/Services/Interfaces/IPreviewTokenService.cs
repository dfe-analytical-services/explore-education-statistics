using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IPreviewTokenService
{
    Task<PreviewToken?> GetPreviewTokenFromRequest();

    Task<bool> ValidatePreviewTokenForDataSet(
        Guid dataSetId,
        CancellationToken cancellationToken = default
    );

    Task<bool> ValidatePreviewTokenForDataSetVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default
    );
}
