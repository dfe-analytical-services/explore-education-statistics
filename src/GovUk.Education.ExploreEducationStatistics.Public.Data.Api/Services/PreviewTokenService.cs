using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class PreviewTokenService(PublicDataDbContext publicDataDbContext) : IPreviewTokenService
{
    public async Task<bool> ValidatePreviewToken(
        string previewToken,
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(previewToken, out var previewTokenId))
        {
            return false;
        }

        return await publicDataDbContext.PreviewTokens
            .Where(pt => pt.Id == previewTokenId)
            .Where(pt => pt.DataSetVersionId == dataSetVersionId)
            .Where(pt => pt.DataSetVersion.Status == DataSetVersionStatus.Draft)
            .Where(pt => pt.Expiry > DateTimeOffset.UtcNow)
            .AnyAsync(cancellationToken: cancellationToken);
    }
}
