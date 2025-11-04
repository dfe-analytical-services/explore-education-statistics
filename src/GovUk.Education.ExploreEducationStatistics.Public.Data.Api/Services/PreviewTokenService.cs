using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils.Requests;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class PreviewTokenService(PublicDataDbContext publicDataDbContext, IHttpContextAccessor httpContextAccessor)
    : IPreviewTokenService
{
    public async Task<PreviewToken?> GetPreviewTokenFromRequest()
    {
        var tokenId = GetPreviewTokenIdFromRequest();

        if (tokenId == null)
        {
            return null;
        }

        return await publicDataDbContext.PreviewTokens.SingleOrDefaultAsync(pt => pt.Id == tokenId);
    }

    public async Task<bool> ValidatePreviewTokenForDataSet(
        Guid dataSetId,
        CancellationToken cancellationToken = default
    )
    {
        var previewTokenId = GetPreviewTokenIdFromRequest();

        if (previewTokenId is null)
        {
            return false;
        }

        var dataSet = await publicDataDbContext
            .DataSets.AsNoTracking()
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
            previewTokenId: previewTokenId.Value,
            cancellationToken: cancellationToken
        );
    }

    public async Task<bool> ValidatePreviewTokenForDataSetVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default
    )
    {
        var previewTokenId = GetPreviewTokenIdFromRequest();

        if (previewTokenId is null)
        {
            return false;
        }

        return await HasActivePreviewToken(
            dataSetVersionId: dataSetVersionId,
            previewTokenId: previewTokenId.Value,
            cancellationToken: cancellationToken
        );
    }

    private async Task<bool> HasActivePreviewToken(
        Guid dataSetVersionId,
        Guid previewTokenId,
        CancellationToken cancellationToken
    )
    {
        var timestamp = DateTimeOffset.UtcNow; // Capture UtcNow and pass into query as a literal to avoid bizarre timing issues in tests.
        return await publicDataDbContext
            .PreviewTokens.Where(pt => pt.Id == previewTokenId)
            .Where(pt => pt.DataSetVersionId == dataSetVersionId)
            .Where(pt => pt.DataSetVersion.Status == DataSetVersionStatus.Draft)
            .Where(pt => pt.Activates <= timestamp && pt.Expires > timestamp)
            .AnyAsync(cancellationToken: cancellationToken);
    }

    private Guid? GetPreviewTokenIdFromRequest()
    {
        if (
            !httpContextAccessor.HttpContext.TryGetRequestHeader(
                RequestHeaderNames.PreviewToken,
                out var previewTokenString
            )
        )
        {
            return null;
        }

        if (!Guid.TryParse(previewTokenString, out var previewTokenId))
        {
            return null;
        }

        return previewTokenId;
    }
}
