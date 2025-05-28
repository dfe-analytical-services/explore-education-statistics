using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils.Requests;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class AnalyticsService(
    IAnalyticsManager analyticsManager,
    IPreviewTokenService previewTokenService,
    PublicDataDbContext publicDataDbContext,
    DateTimeProvider dateTimeProvider,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AnalyticsService> logger) : IAnalyticsService
{
    public async Task CaptureDataSetVersionCall(
        Guid dataSetVersionId,
        DataSetVersionCallType type,
        string? requestedDataSetVersion,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Filter out any non-public calls from analytics.
            if (!IncludeAnalyticsCall())
            {
                logger.LogDebug(
                    message: """
                             Ignoring capturing analytics for "{Type}" call for DataSetVersion {Id}
                             for privileged user's call.
                             """,
                    type,
                    dataSetVersionId);
                return;
            }

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
        catch (Exception e)
        {
            logger.LogError(
                exception: e,
                message: """Error whilst capturing analytics for "{Type}" call for DataSetVersion {Id}""",
                type,
                dataSetVersionId);
        }
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

    private bool IncludeAnalyticsCall()
    {
        return !httpContextAccessor
            .HttpContext
            .TryGetRequestHeader(RequestHeaderNames.RequestSource, out _);
    }
}

public class NoOpAnalyticsService : IAnalyticsService
{
    public Task CaptureDataSetVersionCall(
        Guid dataSetVersionId,
        DataSetVersionCallType type,
        string? requestedDataSetVersion,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
