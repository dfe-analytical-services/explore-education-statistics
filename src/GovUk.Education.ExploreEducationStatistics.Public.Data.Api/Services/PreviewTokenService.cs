using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class PreviewTokenService(PublicDataDbContext publicDataDbContext) : IPreviewTokenService
{
    public async Task<bool> ValidatePreviewTokenForDataSet(
        string previewToken,
        Guid dataSetId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(previewToken, out var previewTokenId))
        {
            return false;
        }

        var dataSet = await publicDataDbContext
            .DataSets
            .AsNoTracking()
            .SingleOrDefaultAsync(d => d.Id == dataSetId, cancellationToken);

        if (dataSet == null)
        {
            return false;
        }

        if (dataSet.LatestDraftVersionId == null)
        {
            return false;
        }

        return await HasActivePreviewToken(
            dataSetVersionId: dataSet.LatestDraftVersionId.Value,
            previewTokenId: previewTokenId,
            cancellationToken: cancellationToken);
    }
    
    public async Task<bool> ValidatePreviewTokenForDataSetVersion(
        string previewToken,
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(previewToken, out var previewTokenId))
        {
            return false;
        }

        return await HasActivePreviewToken(
            dataSetVersionId: dataSetVersionId,
            previewTokenId: previewTokenId,
            cancellationToken: cancellationToken);
    }

    private async Task<bool> HasActivePreviewToken(
        Guid dataSetVersionId,
        Guid previewTokenId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext.PreviewTokens
            .Where(pt => pt.Id == previewTokenId)
            .Where(pt => pt.DataSetVersionId == dataSetVersionId)
            .Where(pt => pt.DataSetVersion.Status == DataSetVersionStatus.Draft)
            .Where(pt => pt.Expiry > DateTimeOffset.UtcNow)
            .AnyAsync(cancellationToken: cancellationToken);
    }
}
