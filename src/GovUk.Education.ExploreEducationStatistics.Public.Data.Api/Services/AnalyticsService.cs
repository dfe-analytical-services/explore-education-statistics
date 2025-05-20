using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class AnalyticsService(
    IAnalyticsManager analyticsManager,
    IPreviewTokenService previewTokenService,
    PublicDataDbContext publicDataDbContext,
    DateTimeProvider dateTimeProvider) : IAnalyticsService
{
    public async Task CaptureDataSetVersionCall(
        Guid dataSetVersionId,
        DataSetVersionCallType type,
        string? requestedDataSetVersion,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var dataSetVersion = await publicDataDbContext
            .DataSetVersions
            .Include(dsv => dsv.DataSet)
            .SingleAsync(dsv => dsv.Id == dataSetVersionId, cancellationToken);

        var request = new CaptureDataSetVersionCallRequest(
            DataSetId: dataSetVersion.DataSetId,
            DataSetVersionId: dataSetVersion.Id,
            DataSetVersion: dataSetVersion.SemVersion().ToString(),
            DataSetTitle: dataSetVersion.DataSet.Title,
            Parameters: parameters,
            PreviewToken: await GetPreviewTokenRequest(),
            RequestedDataSetVersion: requestedDataSetVersion,
            StartTime: dateTimeProvider.UtcNow,
            Type: type);
        
        await analyticsManager.Add(request, cancellationToken);
    }

    private async Task<PreviewTokenRequest?> GetPreviewTokenRequest()
    {
        var previewToken = await previewTokenService.GetPreviewTokenFromRequest();

        if (previewToken == null)
        {
            return null;
        }

        return new PreviewTokenRequest(
            Label: previewToken.Label,
            DataSetVersionId: previewToken.DataSetVersionId,
            Created: previewToken.Created,
            Expiry: previewToken.Expiry);
    }
}
