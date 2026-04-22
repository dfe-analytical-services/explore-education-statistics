using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;

/// <summary>
/// This interface acts as a facade behind which the Screener API is
/// interacted with using a mix of communication techniques, such as queue
/// triggers and HTTP requests.
/// </summary>
public interface IDataSetScreenerService
{
    Task<DataSetScreenerResponse> ScreenDataSet(
        DataSetScreenerRequest dataSetScreenerRequest,
        CancellationToken cancellationToken
    );

    Task StartScreening(DataSetStartScreeningRequest dataSetScreenRequest, CancellationToken cancellationToken);

    Task<List<DataSetScreenerProgressResponse>> UpdateScreeningProgress(CancellationToken cancellationToken);
}
