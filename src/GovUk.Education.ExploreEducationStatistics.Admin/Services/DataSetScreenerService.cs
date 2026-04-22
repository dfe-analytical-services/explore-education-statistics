using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataSetScreenerService(
    IDataSetScreenerClient dataSetScreenerClient,
    [FromKeyedServices(nameof(DataSetScreenerService))] IQueueServiceClient queueServiceClient,
    ContentDbContext contentDbContext
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
        var dataSetsUndergoingScreening = await contentDbContext
            .DataSetUploads.Where(upload => upload.Status == DataSetUploadStatus.SCREENING)
            .Include(upload => upload.ScreeningProgress)
            .ToListAsync(cancellationToken: cancellationToken);

        var dataSetIdsUndergoingScreening = dataSetsUndergoingScreening.Select(upload => upload.Id).ToList();

        var progressResults = await dataSetScreenerClient.GetScreeningProgress(
            dataSetIdsUndergoingScreening,
            cancellationToken
        );

        progressResults.ForEach(progressUpdate =>
        {
            var dataSetToUpdate = dataSetsUndergoingScreening.First(dataSet => dataSet.Id == progressUpdate.DataSetId);

            dataSetToUpdate.ScreeningProgress ??= new DataSetScreeningProgress();
            dataSetToUpdate.ScreeningProgress.PercentageComplete = (int)progressUpdate.PercentageComplete;
            dataSetToUpdate.ScreeningProgress.Stage = progressUpdate.Stage;
            dataSetToUpdate.ScreeningProgress.Completed = progressUpdate.Completed;
            dataSetToUpdate.ScreeningProgress.Passed = progressUpdate.Passed;
        });

        contentDbContext.DataSetUploads.UpdateRange(dataSetsUndergoingScreening);
        await contentDbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return progressResults;
    }
}
