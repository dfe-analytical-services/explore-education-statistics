using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataSetScreenerService(IDataSetScreenerClient dataSetScreenerClient, IOptions<DataScreenerOptions> options)
    : IDataSetScreenerService
{
    private readonly IQueueServiceClient _queueServiceClient = new QueueServiceClient(options.Value.ScreenerStorage);

    public Task<DataSetScreenResponse> ScreenDataSet(
        DataSetScreenRequest dataSetScreenRequest,
        CancellationToken cancellationToken
    )
    {
        return dataSetScreenerClient.ScreenDataSet(dataSetScreenRequest, cancellationToken);
    }

    public async Task StartScreening(
        DataSetStartScreeningRequest dataSetScreenRequest,
        CancellationToken cancellationToken
    )
    {
        await _queueServiceClient.SendMessageAsJson(
            queueName: "start-screening",
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
