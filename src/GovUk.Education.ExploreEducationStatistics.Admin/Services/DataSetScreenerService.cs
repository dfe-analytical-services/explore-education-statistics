using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataSetScreenerService(
    IDataSetScreenerClient dataSetScreenerClient,
    [FromKeyedServices(nameof(IDataSetScreenerClient))] IQueueServiceClient queueServiceClient
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

    public Task<List<DataSetScreenerProgressResponse>> GetScreeningProgress(
        IList<Guid> dataSetIds,
        CancellationToken cancellationToken
    )
    {
        return dataSetScreenerClient.GetScreeningProgress(dataSetIds, cancellationToken);
    }

    public Task DeleteScreeningProgress(IList<Guid> dataSetIds, CancellationToken cancellationToken)
    {
        return dataSetScreenerClient.DeleteScreeningProgress(dataSetIds, cancellationToken);
    }
}
