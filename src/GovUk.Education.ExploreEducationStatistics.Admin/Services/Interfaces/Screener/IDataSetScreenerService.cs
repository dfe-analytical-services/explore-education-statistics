using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;

public interface IDataSetScreenerService
{
    Task<DataSetScreenerResponse> ScreenDataSet(
        DataSetScreenerRequest dataSetScreenerRequest,
        CancellationToken cancellationToken
    );

    Task StartScreening(DataSetStartScreeningRequest dataSetScreenRequest, CancellationToken cancellationToken);

    Task<List<DataSetScreenerProgressResponse>> GetScreeningProgress(
        IList<Guid> dataSetIds,
        CancellationToken cancellationToken
    );

    Task DeleteScreeningProgress(IList<Guid> dataSetIds, CancellationToken cancellationToken);
}
