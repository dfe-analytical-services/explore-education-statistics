#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Responses.Screener;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Screener;

public class DataSetScreenerService(
    IDataSetScreenerClient dataSetScreenerClient,
    [FromKeyedServices(nameof(DataSetScreenerService))] IQueueServiceClient queueServiceClient,
    IUserService userService,
    ContentDbContext contentDbContext,
    TimeProvider timeProvider,
    IMapper mapper,
    IOptions<DataScreenerOptions> options
) : IDataSetScreenerService
{
    public const string StartScreeningQueue = "start-screening";

    public Task<DataSetScreenerResponse> ScreenDataSet(
        DataSetScreenerRequest dataSetScreenerRequest,
        CancellationToken cancellationToken
    )
    {
        return dataSetScreenerClient.ScreenDataSet(dataSetScreenerRequest, cancellationToken);
    }

    public async Task StartScreening(
        DataSetStartScreeningRequest dataSetScreenRequest,
        CancellationToken cancellationToken
    )
    {
        await queueServiceClient.SendMessageAsJson(
            queueName: StartScreeningQueue,
            dataSetScreenRequest,
            cancellationToken
        );
    }

    public async Task<List<DataSetScreenerProgressResponse>> UpdateScreeningProgress(
        CancellationToken cancellationToken
    )
    {
        var utcNow = timeProvider.GetUtcNow();
        var lastCheckedWindow = utcNow.AddSeconds(-options.Value.ScreenerProgressUpdateIntervalSeconds);

        // Find any data sets currently undergoing screening that haven't had a progress check made for
        // them in the last <ScreenerProgressUpdateIntervalSeconds> seconds.
        var dataSetsNeedingScreenerProgressUpdates = await contentDbContext
            .DataSetUploads.Where(upload => upload.Status == DataSetUploadStatus.SCREENING)
            .Where(upload =>
                upload.ScreenerProgressLastChecked == null || upload.ScreenerProgressLastChecked <= lastCheckedWindow
            )
            .ToListAsync(cancellationToken: cancellationToken);

        if (dataSetsNeedingScreenerProgressUpdates.Count == 0)
        {
            return [];
        }

        var dataSetIdsUndergoingScreening = dataSetsNeedingScreenerProgressUpdates.Select(upload => upload.Id).ToList();

        // Request progress updates for all data sets requiring one.
        var progressResults = await dataSetScreenerClient.GetScreenerProgress(
            dataSetIdsUndergoingScreening,
            cancellationToken
        );

        // For each data set that required a progress update, attempt to update its progress
        // based on the responses received from the Screener API.
        dataSetsNeedingScreenerProgressUpdates.ForEach(dataSetToUpdate =>
        {
            var progressUpdateForDataSet = progressResults.SingleOrDefault(result =>
                result.DataSetId == dataSetToUpdate.Id
            );

            var progressReport = progressUpdateForDataSet?.ProgressReport;

            // If a progress response was returned from the Screener API for this
            // data set, apply it.
            if (progressReport != null && ScreenerProgressHasChanged(dataSetToUpdate.ScreenerProgress, progressReport))
            {
                // Only update Screener progress details if some actual progress has been made since the
                // last time the progress was updated. This helps to identify and clean up any stalled
                // screening attempts.
                if (ScreenerProgressHasChanged(dataSetToUpdate.ScreenerProgress, progressReport))
                {
                    dataSetToUpdate.ScreenerProgress!.PercentageComplete = progressReport.PercentageComplete;
                    dataSetToUpdate.ScreenerProgress.Stage = progressReport.Stage;
                    dataSetToUpdate.ScreenerProgress.Completed = progressReport.Completed;
                    dataSetToUpdate.ScreenerProgress.Passed = progressReport.Passed;

                    // Mark the data set as having received a successful progress update.
                    dataSetToUpdate.ScreenerProgressLastUpdated = utcNow;
                }
            }

            // Update the "last checked" date to mark the last time that progress was checked
            // for this data set.
            dataSetToUpdate.ScreenerProgressLastChecked = utcNow;
        });

        contentDbContext.DataSetUploads.UpdateRange(dataSetsNeedingScreenerProgressUpdates);
        await contentDbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return progressResults;
    }

    public async Task<List<DataSetUploadViewModel>> MarkDataSetsWithoutProgressAsFailed(
        CancellationToken cancellationToken
    )
    {
        var progressUpdatesFailureIntervalMins = options.Value.ScreenerProgressUpdateFailureIntervalMinutes;

        // Find all data sets currently undergoing screening.
        var screeningDataSets = await contentDbContext
            .DataSetUploads.Where(upload => upload.Status == DataSetUploadStatus.SCREENING)
            .ToListAsync(cancellationToken: cancellationToken);

        // Find any data sets where the gap between their last successful progress update
        // (or the very first attempt to get a progress update), and the last time that Admin checked
        // for a progress update, has become too large.
        var screeningDataSetsWithoutRecentProgressUpdates = screeningDataSets
            .Where(upload =>
                upload is { ScreenerProgressLastChecked: not null, ScreenerProgressLastUpdated: not null }
                && (upload.ScreenerProgressLastChecked.Value - upload.ScreenerProgressLastUpdated.Value).TotalMinutes
                    >= progressUpdatesFailureIntervalMins
            )
            .ToList();

        if (screeningDataSetsWithoutRecentProgressUpdates.Count == 0)
        {
            return [];
        }

        // For each affected data set, mark it as having failed screening due to not receiving
        // a successful progress update for too long.
        screeningDataSetsWithoutRecentProgressUpdates.ForEach(upload =>
        {
            upload.ScreenerResult = new DataSetScreenerResponse
            {
                Passed = false,
                OverallResult = "Failed to retrieve progress updates",
                TestResults = [],
                PublicApiCompatible = false,
            };
            upload.Status = DataSetUploadStatus.SCREENER_ERROR;
            contentDbContext.DataSetUploads.Update(upload);
        });

        await contentDbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        // Ask the Screener API to delete any progress files that it may have for any of the data sets
        // that we marked as failed, if any exist for them.
        var dataSetIdsMarkedAsFailed = screeningDataSetsWithoutRecentProgressUpdates
            .Select(upload => upload.Id)
            .ToList();

        await dataSetScreenerClient.DeleteScreenerProgressAndCompletionFiles(
            dataSetIds: dataSetIdsMarkedAsFailed,
            cancellationToken: cancellationToken
        );

        return [.. screeningDataSetsWithoutRecentProgressUpdates.Select(mapper.Map<DataSetUploadViewModel>)];
    }

    public Task<Either<ActionResult, List<ScreenerProgressWithDataSetUploadIdViewModel>>> GetScreenerProgress(
        Guid releaseVersionId,
        CancellationToken cancellationToken
    )
    {
        return contentDbContext
            .ReleaseVersions.SingleOrNotFound(rv => rv.Id == releaseVersionId)
            .OnSuccess(userService.CheckCanViewReleaseVersion)
            .OnSuccess(async () =>
            {
                var dataSetsUndergoingScreening = await contentDbContext
                    .DataSetUploads.Where(upload =>
                        upload.ReleaseVersionId == releaseVersionId && upload.Status == DataSetUploadStatus.SCREENING
                    )
                    .ToListAsync(cancellationToken: cancellationToken);

                return dataSetsUndergoingScreening
                    .Select(upload => new ScreenerProgressWithDataSetUploadIdViewModel
                    {
                        DataSetUploadId = upload.Id,
                        PercentageComplete = (int)
                            Math.Round(upload.ScreenerProgress?.PercentageComplete ?? 0, MidpointRounding.ToZero),
                        Stage = upload.ScreenerProgress?.Stage,
                    })
                    .ToList();
            });
    }

    public async Task<List<DataSetUploadViewModel>> CompleteDataSetScreeningForFinishedDataSets(
        CancellationToken cancellationToken
    )
    {
        var dataSetsCompletedScreening = await GetDataSetsThatHaveCompletedScreening(cancellationToken);

        if (dataSetsCompletedScreening.Count == 0)
        {
            return [];
        }

        var dataSetIds = dataSetsCompletedScreening.Select(upload => upload.Id).ToList();

        // Get the full report on their completion statuses.
        var completionReports = await dataSetScreenerClient.GetScreenerCompletionReports(
            dataSetIds: dataSetIds,
            cancellationToken: cancellationToken
        );

        // For every completion report in the response, attach it to the appropriate upload, and update its status
        // to be a post-screening status. This way, successful ones will no longer be polled for progress updates,
        // and stalled ones will eventually be cleaned up by MarkDataSetsWithoutProgressAsFailed() if we cannot get
        // a completion report successfully for them within a time limit.
        var dataSetsToComplete = dataSetsCompletedScreening
            .Where(upload => completionReports.Any(report => report.DataSetId == upload.Id))
            .ToList();

        // If we don't have any completion reports for our completed data sets yet, return early.
        if (dataSetsToComplete.Count == 0)
        {
            return [];
        }

        dataSetsToComplete.ForEach(uploadToComplete =>
        {
            var report = completionReports.Single(u => u.DataSetId == uploadToComplete.Id);

            uploadToComplete.ScreenerResult = report.CompletionReport;
            uploadToComplete.Status = report.CompletionReport.Passed
                ? DataSetUploadStatus.PENDING_REVIEW
                : DataSetUploadStatus.FAILED_SCREENING;
            contentDbContext.DataSetUploads.Update(uploadToComplete);
        });

        await contentDbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        // For all uploads that were successfully marked as having completed screening, call the Screener API to clean
        // up its progress and completion report files.
        var dataSetIdsWithSuccessfulCompletionReports = dataSetsToComplete.Select(upload => upload.Id).ToList();

        await dataSetScreenerClient.DeleteScreenerProgressAndCompletionFiles(
            dataSetIds: dataSetIdsWithSuccessfulCompletionReports,
            cancellationToken: cancellationToken
        );

        return [.. dataSetsToComplete.Select(mapper.Map<DataSetUploadViewModel>)];
    }

    private async Task<List<DataSetUpload>> GetDataSetsThatHaveCompletedScreening(CancellationToken cancellationToken)
    {
        // Find all data sets currently undergoing screening.
        var screeningDataSets = await contentDbContext
            .DataSetUploads.Where(upload => upload.Status == DataSetUploadStatus.SCREENING)
            .ToListAsync(cancellationToken: cancellationToken);

        // Find only those whose latest progress reports show as having been completed.
        return [.. screeningDataSets.Where(upload => upload.ScreenerProgress is { Completed: true })];
    }

    private bool ScreenerProgressHasChanged(
        DataSetScreenerProgress? currentScreenerProgress,
        DataSetScreenerProgressReport progressReport
    )
    {
        if (currentScreenerProgress == null)
        {
            return true;
        }

        return Math.Abs(currentScreenerProgress.PercentageComplete - progressReport.PercentageComplete) > 0.001
            || currentScreenerProgress.Stage != progressReport.Stage
            || currentScreenerProgress.Passed != progressReport.Passed
            || currentScreenerProgress.Completed != progressReport.Completed;
    }
}
