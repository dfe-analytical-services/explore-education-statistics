using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Responses.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Screener;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;

public interface IDataSetScreenerClient
{
    Task<DataSetScreenerResponse> ScreenDataSet(
        DataSetScreenerRequest dataSetScreenerRequest,
        CancellationToken cancellationToken
    );

    Task<List<DataSetScreenerProgressResponse>> GetScreenerProgress(
        IList<Guid> dataSetIds,
        CancellationToken cancellationToken
    );
}
