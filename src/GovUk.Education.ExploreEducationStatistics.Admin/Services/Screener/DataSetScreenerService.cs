using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Responses.Screener;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
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

        var dataSetsUndergoingScreening = await contentDbContext
            .DataSetUploads.Where(upload => upload.Status == DataSetUploadStatus.SCREENING)
            .Where(upload =>
                upload.ScreenerProgressLastChecked == null || upload.ScreenerProgressLastChecked <= lastCheckedWindow
            )
            .ToListAsync(cancellationToken: cancellationToken);

        if (dataSetsUndergoingScreening.Count == 0)
        {
            return [];
        }

        var dataSetIdsUndergoingScreening = dataSetsUndergoingScreening.Select(upload => upload.Id).ToList();

        var progressResults = await dataSetScreenerClient.GetScreenerProgress(
            dataSetIdsUndergoingScreening,
            cancellationToken
        );

        // For each progress update received, update the screener progress details on the
        // associated DataSetUpload entry.
        dataSetsUndergoingScreening.ForEach(dataSetToUpdate =>
        {
            dataSetToUpdate.ScreenerProgress ??= new DataSetScreenerProgress
            {
                PercentageComplete = 0,
                Completed = false,
                Passed = false,
                Stage = "",
            };

            var progressUpdateForDataSet = progressResults.SingleOrDefault(result =>
                result.DataSetId == dataSetToUpdate.Id
            );

            if (progressUpdateForDataSet != null)
            {
                dataSetToUpdate.ScreenerProgress.PercentageComplete = (int)
                    Math.Round(progressUpdateForDataSet.PercentageComplete, MidpointRounding.ToZero);
                dataSetToUpdate.ScreenerProgress.Stage = progressUpdateForDataSet.Stage;
                dataSetToUpdate.ScreenerProgress.Completed = progressUpdateForDataSet.Completed;
                dataSetToUpdate.ScreenerProgress.Passed = progressUpdateForDataSet.Passed;

                // Update the "last updated" date to mark the last time that a progress update was
                // successfully applied for this data set.
                dataSetToUpdate.ScreenerProgressLastUpdated = utcNow;
            }

            // Update the "last checked" date to mark the last time that progress was checked
            // for this data set.
            dataSetToUpdate.ScreenerProgressLastChecked = utcNow;
        });

        contentDbContext.DataSetUploads.UpdateRange(dataSetsUndergoingScreening);
        await contentDbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return progressResults;
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
                        PercentageComplete = upload.ScreenerProgress?.PercentageComplete ?? 0,
                        Stage = upload.ScreenerProgress?.Stage,
                    })
                    .ToList();
            });
    }
}
