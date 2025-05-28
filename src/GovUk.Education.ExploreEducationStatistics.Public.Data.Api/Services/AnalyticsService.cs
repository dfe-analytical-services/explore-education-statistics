using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class AnalyticsService(
    IAnalyticsManager analyticsManager,
    IPreviewTokenService previewTokenService,
    IAuthorizationService authorizationService,
    PublicDataDbContext publicDataDbContext,
    DateTimeProvider dateTimeProvider,
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
            if (!IncludeAnalyticsCall())
            {
                logger.LogDebug(
                    message: """
                             Ignoring capturing analytics for "{Type}" call for DataSetVersion {Id}.
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
    
    public async Task CaptureDataSetVersionQuery(
        DataSetVersion dataSetVersion,
        string? requestedDataSetVersion,
        DataSetQueryRequest query,
        DataSetQueryPaginatedResultsViewModel results,
        DateTime startTime,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!IncludeAnalyticsCall())
            {
                logger.LogDebug(
                    message: "Ignoring capturing analytics for Public API query for DataSetVersion {Id}.",
                    dataSetVersion.Id);
                return;
            }

            await analyticsManager.Add(
                request: new CaptureDataSetVersionQueryRequest(
                    DataSetId: dataSetVersion.DataSetId,
                    DataSetVersionId: dataSetVersion.Id,
                    DataSetVersion: dataSetVersion.SemVersion().ToString(),
                    DataSetTitle: dataSetVersion.DataSet.Title,
                    PreviewToken: await GetPreviewTokenRequest(),
                    RequestedDataSetVersion: requestedDataSetVersion,
                    Query: query,
                    ResultsCount: results.Results.Count,
                    TotalRowsCount: results.Paging.TotalResults,
                    StartTime: startTime,
                    EndTime: DateTime.UtcNow),
                cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(
                exception: e,
                message: "Error whilst capturing analytics for Public API query for DataSetVersion {Id}",
                dataSetVersion.Id);
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

    /// <summary>
    /// Filter out any non-public calls from analytics.
    /// </summary>
    private bool IncludeAnalyticsCall()
    {
        return !authorizationService.CanAccessUnpublishedData();
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

    public Task CaptureDataSetVersionQuery(
        DataSetVersion dataSetVersion,
        string? requestedDataSetVersion,
        DataSetQueryRequest query,
        DataSetQueryPaginatedResultsViewModel results,
        DateTime startTime,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
