using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
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
    public async Task CaptureDataSetCall(
        Guid dataSetId,
        DataSetCallType type,
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
                             Ignoring capturing analytics for "{Type}" call for DataSet {Id}.
                             """,
                    type,
                    dataSetId);
                return;
            }

            var dataSet = await publicDataDbContext
                .DataSets
                .SingleAsync(ds => ds.Id == dataSetId, cancellationToken);

            var request = new CaptureDataSetCallRequest(
                DataSetId: dataSet.Id,
                DataSetTitle: dataSet.Title,
                Parameters: parameters,
                PreviewToken: await GetPreviewTokenRequest(),
                StartTime: dateTimeProvider.UtcNow,
                Type: type);

            await analyticsManager.Add(request, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(
                exception: e,
                message: """Error whilst capturing analytics for "{Type}" call for DataSet {Id}""",
                type,
                dataSetId);
        }
    }
    
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
            // Filter out any calls from analytics that originate from the EES service itself
            // so that we're capturing only those made by public users.
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
    /// Filter out any calls from analytics that originate from the EES service itself
    /// so that we're capturing only those made by public users.
    /// </summary>
    private bool IncludeAnalyticsCall()
    {
        return !httpContextAccessor
            .HttpContext
            .TryGetRequestHeader(RequestHeaderNames.RequestSource, out _);
    }
}

public class NoOpAnalyticsService : IAnalyticsService
{
    public Task CaptureDataSetCall(
        Guid dataSetId,
        DataSetCallType type,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

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
